using System.Text;
using System.Text.Json;
using ManConstruc.Api.Contracts;

namespace ManConstruc.Api.Services;

public class DeepseekPricingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public DeepseekPricingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Deepseek:ApiKey"]
            ?? throw new InvalidOperationException("Falta configurar Deepseek:ApiKey.");
        _model = configuration["Deepseek:Model"] ?? "deepseek-chat";
    }

    public async Task<WorkInput> EstimatePricesAsync(WorkInput input, CancellationToken cancellationToken = default)
    {
        var requestJson = JsonSerializer.Serialize(input, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var prompt = $$"""
Eres un estimador de costos de construccion en Colombia (moneda COP).
Se te entrega una obra en JSON. Debes estimar precios unitarios realistas para:
- UnitPrice de cada material.
- UnitRate de cada mano de obra.
Mantén exactamente la misma estructura, cantidades, descripciones, unidades y nombres.
Solo completa o ajusta los valores de UnitPrice y UnitRate.
Responde unicamente con JSON valido, sin markdown ni explicaciones.
Obra:
{requestJson}
""";

        var deepseekRequest = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2,
            response_format = new { type = "json_object" }
        };

        var url = $"https://api.deepseek.com/v1/chat/completions";
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        using var response = await _httpClient.PostAsJsonAsync(url, deepseekRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        var deepseekResponse = JsonSerializer.Deserialize<DeepseekResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("La IA no devolvio contenido.");

        var text = deepseekResponse?.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("La IA no devolvio contenido.");

        var json = ExtractJson(text);
        var estimated = JsonSerializer.Deserialize<WorkInput>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("No se pudo interpretar la respuesta de la IA.");

        return Merge(input, estimated);
    }

    public async Task<WorkInput> GenerateWorkFromDescriptionAsync(string description, CancellationToken cancellationToken = default)
    {
        var laborTabulator = """
Tabulador de mano de obra (COP, referencia 2026):
- Albañilería general: 18000-25000 COP/h
- Pintura (aplicación): 12000-18000 COP/m2
- Plantilla de piso (nivelación): 8000-12000 COP/m2
- Enchape: 25000-35000 COP/m2
- Pañete: 12000-18000 COP/m2
- Instalación eléctrica: 30000-45000 COP/punto
- Plomería: 40000-60000 COP/punto
""";

        var prompt = $$"""
Eres un estimador de costos de construcción en Colombia (moneda COP).
El usuario describe un trabajo en lenguaje natural: "{{description}}".

Debes generar un presupuesto completo en JSON con la misma estructura WorkInput:
- name: nombre corto del trabajo.
- clientName, location, description: vacíos o inferidos.
- currency implícita COP.
- items: lista de partidas. Para cada partida:
  - name, unit, quantity, wastePercent, notes.
  - materials: lista de materiales/herramientas necesarias con description, unit, quantityPerUnit, unitPrice.
  - labors: lista de mano de obra con description, unit, quantityPerUnit, unitRate.

Reglas:
1. Infiere los materiales y herramientas necesarios (ej. pintura, brochas, rodillos, thinner) y sus cantidades según las dimensiones dadas.
2. Los unitPrice de materiales deben ser precios realistas de tiendas colombianas muy cotizadas para estos rubros (Homecenter, Easy, Corona, etc.) vigentes en 2026.
3. Los unitRate de mano de obra deben basarse en el siguiente tabulador:
{{laborTabulator}}
4. Si el trabajo es "hacer una plantilla de piso", incluye cemento, arena, piedra, agua y mano de obra de nivelación con precios de tiendas.
5. Mantén cantidades coherentes con las dimensiones.
6. Responde ÚNICAMENTE con JSON válido, sin markdown ni explicaciones.

Estructura exacta:
{
  "name": "...",
  "clientName": "",
  "location": "",
  "description": "...",
  "items": [
    {
      "name": "...",
      "unit": "...",
      "quantity": 0,
      "wastePercent": 0,
      "notes": "...",
      "materials": [
        { "description": "...", "unit": "...", "quantityPerUnit": 0, "unitPrice": 0 }
      ],
      "labors": [
        { "description": "...", "unit": "...", "quantityPerUnit": 0, "unitRate": 0 }
      ]
    }
  ]
}
""";

        var deepseekRequest = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            response_format = new { type = "json_object" }
        };

        var url = $"https://api.deepseek.com/v1/chat/completions";
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        using var response = await _httpClient.PostAsJsonAsync(url, deepseekRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        var deepseekResponse = JsonSerializer.Deserialize<DeepseekResponse>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("La IA no devolvio contenido.");

        var text = deepseekResponse?.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("La IA no devolvio contenido.");

        var json = ExtractJson(text);
        var generated = JsonSerializer.Deserialize<WorkInput>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("No se pudo interpretar la respuesta de la IA.");

        return NormalizeGeneratedWork(generated, description);
    }

    private static WorkInput NormalizeGeneratedWork(WorkInput generated, string description)
    {
        if (string.IsNullOrWhiteSpace(generated.Name))
        {
            generated.Name = description.Length > 60 ? description.Substring(0, 60) : description;
        }

        if (generated.Items is null)
        {
            generated.Items = new List<WorkItemInput>();
        }

        foreach (var item in generated.Items)
        {
            if (item.Materials is null) item.Materials = new List<MaterialInput>();
            if (item.Labors is null) item.Labors = new List<LaborInput>();
            if (string.IsNullOrWhiteSpace(item.Unit)) item.Unit = "un";
            foreach (var material in item.Materials)
            {
                if (string.IsNullOrWhiteSpace(material.Unit)) material.Unit = "un";
                if (material.UnitPrice < 0) material.UnitPrice = 0;
            }
            foreach (var labor in item.Labors)
            {
                if (string.IsNullOrWhiteSpace(labor.Unit)) labor.Unit = "h";
                if (labor.UnitRate < 0) labor.UnitRate = 0;
            }
        }

        return generated;
    }

    private static WorkInput Merge(WorkInput original, WorkInput estimated)
    {
        var result = new WorkInput
        {
            Name = original.Name,
            ClientName = original.ClientName,
            Location = original.Location,
            Description = original.Description,
            Items = new List<WorkItemInput>()
        };

        for (var i = 0; i < original.Items.Count; i++)
        {
            var originalItem = original.Items[i];
            var estimatedItem = i < estimated.Items.Count ? estimated.Items[i] : null;
            var item = new WorkItemInput
            {
                Name = originalItem.Name,
                Unit = originalItem.Unit,
                Quantity = originalItem.Quantity,
                WastePercent = originalItem.WastePercent,
                Notes = originalItem.Notes,
                Materials = new List<MaterialInput>(),
                Labors = new List<LaborInput>()
            };

            for (var m = 0; m < originalItem.Materials.Count; m++)
            {
                var material = originalItem.Materials[m];
                var estimatedMaterial = estimatedItem is not null && m < estimatedItem.Materials.Count
                    ? estimatedItem.Materials[m]
                    : null;
                item.Materials.Add(new MaterialInput
                {
                    Description = material.Description,
                    Unit = material.Unit,
                    QuantityPerUnit = material.QuantityPerUnit,
                    UnitPrice = estimatedMaterial is not null && estimatedMaterial.UnitPrice >= 0
                        ? estimatedMaterial.UnitPrice
                        : material.UnitPrice
                });
            }

            for (var l = 0; l < originalItem.Labors.Count; l++)
            {
                var labor = originalItem.Labors[l];
                var estimatedLabor = estimatedItem is not null && l < estimatedItem.Labors.Count
                    ? estimatedItem.Labors[l]
                    : null;
                item.Labors.Add(new LaborInput
                {
                    Description = labor.Description,
                    Unit = labor.Unit,
                    QuantityPerUnit = labor.QuantityPerUnit,
                    UnitRate = estimatedLabor is not null && estimatedLabor.UnitRate >= 0
                        ? estimatedLabor.UnitRate
                        : labor.UnitRate
                });
            }
            result.Items.Add(item);
        }

        return result;
    }

    private static string ExtractJson(string text)
    {
        text = text.Trim();
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return text.Substring(start, end - start + 1);
        }
        return text;
    }

    private class DeepseekResponse
    {
        public List<DeepseekChoice>? Choices { get; set; }
    }

    private class DeepseekChoice
    {
        public DeepseekMessage? Message { get; set; }
    }

    private class DeepseekMessage
    {
        public string? Content { get; set; }
    }
}