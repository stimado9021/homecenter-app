using ManConstruc.Api.Contracts;
using ManConstruc.Api.Services;
using Xunit;

namespace ManConstruc.Tests;

public class WorkMapperTests
{
    [Fact]
    public void Mapea_entrada_y_persiste_los_totales_calculados()
    {
        var input = new WorkInput
        {
            Name = "  Casa lote 12  ",
            ClientName = "  Ana Pérez ",
            Items =
            {
                new WorkItemInput
                {
                    Name = "  Placa  ",
                    Unit = "m2",
                    Quantity = 20,
                    WastePercent = 5,
                    Materials =
                    {
                        new MaterialInput
                        {
                            Description = "  Cemento  ",
                            Unit = "bulto",
                            QuantityPerUnit = 0.4m,
                            UnitPrice = 30000m
                        }
                    },
                    Labors =
                    {
                        new LaborInput
                        {
                            Description = "  Cuadrilla  ",
                            Unit = "h",
                            QuantityPerUnit = 0.8m,
                            UnitRate = 18000m
                        }
                    }
                }
            }
        };

        var work = WorkMapper.CreateEntity(input, Guid.NewGuid());

        Assert.Equal("Casa lote 12", work.Name);
        Assert.Equal("Ana Pérez", work.ClientName);
        Assert.Equal("Placa", work.Items[0].Name);
        Assert.Equal("Cemento", work.Items[0].Materials[0].Description);
        Assert.Equal(252000m, work.MaterialTotal);
        Assert.Equal(288000m, work.LaborTotal);
        Assert.Equal(540000m, work.Total);

        var detail = WorkMapper.ToDetail(work);
        Assert.Equal(540000m, detail.Total);
        Assert.Single(detail.Items);
    }
}
