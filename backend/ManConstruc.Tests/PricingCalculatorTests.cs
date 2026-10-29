using ManConstruc.Api.Models;
using ManConstruc.Api.Services;
using Xunit;

namespace ManConstruc.Tests;

public class PricingCalculatorTests
{
    [Fact]
    public void Calcula_materiales_con_desperiticio_y_mano_de_obra()
    {
        var work = new Work
        {
            Items =
            {
                new WorkItem
                {
                    Name = "Muro en bloque",
                    Unit = "m2",
                    Quantity = 10,
                    WastePercent = 10,
                    Materials =
                    {
                        new MaterialLine
                        {
                            Description = "Bloque",
                            Unit = "und",
                            QuantityPerUnit = 1.05m,
                            UnitPrice = 25000m
                        }
                    },
                    Labors =
                    {
                        new LaborLine
                        {
                            Description = "Albañil",
                            Unit = "h",
                            QuantityPerUnit = 0.5m,
                            UnitRate = 20000m
                        }
                    }
                }
            }
        };

        var result = PricingCalculator.Calculate(work);
        var item = Assert.Single(result.Items);
        var material = Assert.Single(item.Materials);

        Assert.Equal(11.55m, material.TotalQuantity);
        Assert.Equal(288750m, material.LineTotal);
        Assert.Equal(288750m, item.MaterialTotal);
        Assert.Equal(100000m, item.LaborTotal);
        Assert.Equal(388750m, item.Total);
        Assert.Equal(38875m, item.UnitTotal);
        Assert.Equal(388750m, result.Total);
    }

    [Fact]
    public void Suma_totales_por_partida_y_calcula_participacion()
    {
        var work = new Work
        {
            Items =
            {
                new WorkItem
                {
                    Name = "Piso",
                    Unit = "m2",
                    Quantity = 5,
                    Materials =
                    {
                        new MaterialLine { Description = "Cerámica", QuantityPerUnit = 1, UnitPrice = 20000 }
                    },
                    Labors =
                    {
                        new LaborLine { Description = "Instalador", QuantityPerUnit = 1, UnitRate = 10000 }
                    }
                },
                new WorkItem
                {
                    Name = "Pintura",
                    Unit = "m2",
                    Quantity = 10,
                    Materials =
                    {
                        new MaterialLine { Description = "Pintura", QuantityPerUnit = 0.5m, UnitPrice = 10000 }
                    }
                }
            }
        };

        var result = PricingCalculator.Calculate(work);

        Assert.Equal(2, result.ItemCount);
        Assert.Equal(150000m, result.MaterialTotal);
        Assert.Equal(50000m, result.LaborTotal);
        Assert.Equal(200000m, result.Total);
        Assert.Equal(75m, result.MaterialShare);
        Assert.Equal(25m, result.LaborShare);
    }

    [Fact]
    public void Obra_sin_partidas_devuelve_totales_en_cero()
    {
        var result = PricingCalculator.Calculate(new Work());

        Assert.Empty(result.Items);
        Assert.Equal(0m, result.MaterialTotal);
        Assert.Equal(0m, result.LaborTotal);
        Assert.Equal(0m, result.Total);
        Assert.Equal(0m, result.MaterialShare);
        Assert.Equal(0m, result.LaborShare);
    }

    [Fact]
    public void Desperdicio_se_limita_a_cien_por_ciento()
    {
        var work = new Work
        {
            Items =
            {
                new WorkItem
                {
                    Name = "Excavación",
                    Unit = "m3",
                    Quantity = 2,
                    WastePercent = 150,
                    Materials =
                    {
                        new MaterialLine { Description = "Herramienta", QuantityPerUnit = 1, UnitPrice = 1000 }
                    }
                }
            }
        };

        var result = PricingCalculator.Calculate(work);
        var material = Assert.Single(result.Items[0].Materials);

        Assert.Equal(100m, result.Items[0].WastePercent);
        Assert.Equal(4m, material.TotalQuantity);
        Assert.Equal(4000m, material.LineTotal);
    }

    [Fact]
    public void Cantidad_cero_no_genera_division_por_cero()
    {
        var work = new Work
        {
            Items =
            {
                new WorkItem
                {
                    Name = "Sin cantidad",
                    Unit = "un",
                    Quantity = 0,
                    Labors =
                    {
                        new LaborLine { Description = "Ayudante", QuantityPerUnit = 1, UnitRate = 5000 }
                    }
                }
            }
        };

        var result = PricingCalculator.Calculate(work);

        Assert.Equal(0m, result.Items[0].Total);
        Assert.Equal(0m, result.Items[0].UnitTotal);
        Assert.Equal(0m, result.Total);
    }
}
