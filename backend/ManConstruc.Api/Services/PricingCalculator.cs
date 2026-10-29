using ManConstruc.Api.Contracts;
using ManConstruc.Api.Models;

namespace ManConstruc.Api.Services;

public static class PricingCalculator
{
    public static WorkCalculation Calculate(Work work)
    {
        var calculation = new WorkCalculation
        {
            Items = work.Items
                .OrderBy(item => item.SortOrder)
                .Select(CalculateItem)
                .ToList()
        };

        calculation.MaterialTotal = RoundMoney(calculation.Items.Sum(item => item.MaterialTotal));
        calculation.LaborTotal = RoundMoney(calculation.Items.Sum(item => item.LaborTotal));
        calculation.Total = RoundMoney(calculation.MaterialTotal + calculation.LaborTotal);
        calculation.MaterialShare = PercentOf(calculation.MaterialTotal, calculation.Total);
        calculation.LaborShare = PercentOf(calculation.LaborTotal, calculation.Total);

        return calculation;
    }

    public static ItemCalculation CalculateItem(WorkItem item)
    {
        var wastePercent = Math.Clamp(item.WastePercent, 0m, 100m);
        var factor = 1m + wastePercent / 100m;

        var result = new ItemCalculation
        {
            Id = item.Id,
            Name = item.Name,
            Unit = item.Unit,
            Quantity = item.Quantity,
            WastePercent = wastePercent,
            Notes = item.Notes,
            Materials = item.Materials
                .OrderBy(material => material.SortOrder)
                .Select(material =>
                {
                    var totalQuantity = RoundQuantity(item.Quantity * material.QuantityPerUnit * factor);
                    return new MaterialCalculation
                    {
                        Id = material.Id,
                        Description = material.Description,
                        Unit = material.Unit,
                        QuantityPerUnit = material.QuantityPerUnit,
                        UnitPrice = material.UnitPrice,
                        WastePercent = wastePercent,
                        TotalQuantity = totalQuantity,
                        LineTotal = RoundMoney(totalQuantity * material.UnitPrice)
                    };
                })
                .ToList(),
            Labors = item.Labors
                .OrderBy(labor => labor.SortOrder)
                .Select(labor =>
                {
                    var totalQuantity = RoundQuantity(item.Quantity * labor.QuantityPerUnit);
                    return new LaborCalculation
                    {
                        Id = labor.Id,
                        Description = labor.Description,
                        Unit = labor.Unit,
                        QuantityPerUnit = labor.QuantityPerUnit,
                        UnitRate = labor.UnitRate,
                        TotalQuantity = totalQuantity,
                        LineTotal = RoundMoney(totalQuantity * labor.UnitRate)
                    };
                })
                .ToList()
        };

        result.MaterialTotal = RoundMoney(result.Materials.Sum(material => material.LineTotal));
        result.LaborTotal = RoundMoney(result.Labors.Sum(labor => labor.LineTotal));
        result.Total = RoundMoney(result.MaterialTotal + result.LaborTotal);
        result.UnitTotal = item.Quantity <= 0 ? 0m : RoundMoney(result.Total / item.Quantity);

        return result;
    }

    private static decimal PercentOf(decimal part, decimal total) =>
        total <= 0 ? 0m : Math.Round(part / total * 100m, 2, MidpointRounding.AwayFromZero);

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal RoundQuantity(decimal value) =>
        Math.Round(value, 4, MidpointRounding.AwayFromZero);
}
