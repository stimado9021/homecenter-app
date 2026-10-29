using System.ComponentModel.DataAnnotations;

namespace ManConstruc.Api.Contracts;

public class WorkInput
{
    [Required(ErrorMessage = "El nombre de la obra es obligatorio.")]
    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(160)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "Agrega al menos una partida a la obra.")]
    public List<WorkItemInput> Items { get; set; } = new();
}

public class WorkItemInput
{
    [Required(ErrorMessage = "El nombre de la partida es obligatorio.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad de la partida es obligatoria.")]
    [MaxLength(24)]
    public string Unit { get; set; } = "un";

    [Range(0.0001, 1_000_000_000, ErrorMessage = "La cantidad de la partida debe ser mayor que cero.")]
    public decimal Quantity { get; set; } = 1;

    [Range(0, 100, ErrorMessage = "El desperdicio debe estar entre 0 y 100 %.")]
    public decimal WastePercent { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    public List<MaterialInput> Materials { get; set; } = new();
    public List<LaborInput> Labors { get; set; } = new();
}

public class MaterialInput
{
    [Required(ErrorMessage = "La descripción del material es obligatoria.")]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad del material es obligatoria.")]
    [MaxLength(24)]
    public string Unit { get; set; } = "un";

    [Range(0, 1_000_000, ErrorMessage = "La cantidad por unidad no puede ser negativa.")]
    public decimal QuantityPerUnit { get; set; }

    [Range(0, 1_000_000_000, ErrorMessage = "El precio unitario no puede ser negativo.")]
    public decimal UnitPrice { get; set; }
}

public class LaborInput
{
    [Required(ErrorMessage = "La descripción de la mano de obra es obligatoria.")]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad de la mano de obra es obligatoria.")]
    [MaxLength(24)]
    public string Unit { get; set; } = "h";

    [Range(0, 1_000_000, ErrorMessage = "Las horas por unidad no pueden ser negativas.")]
    public decimal QuantityPerUnit { get; set; }

    [Range(0, 1_000_000_000, ErrorMessage = "La tarifa no puede ser negativa.")]
    public decimal UnitRate { get; set; }
}

public record WorkSummaryDto(
    Guid Id,
    string Name,
    string ClientName,
    string Location,
    string Currency,
    int ItemCount,
    decimal MaterialTotal,
    decimal LaborTotal,
    decimal Total,
    DateTime UpdatedAt);

public record WorkGenerationRequest(string Description);

public record WorkDetailDto(
    Guid Id,
    string Name,
    string ClientName,
    string Location,
    string Description,
    string Currency,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ItemCalculation> Items,
    decimal MaterialTotal,
    decimal LaborTotal,
    decimal Total,
    decimal MaterialShare,
    decimal LaborShare);
