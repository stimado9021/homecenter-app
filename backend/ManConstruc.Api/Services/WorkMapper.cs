using ManConstruc.Api.Contracts;
using ManConstruc.Api.Models;

namespace ManConstruc.Api.Services;

public static class WorkMapper
{
    public static Work CreateEntity(WorkInput input, Guid userId)
    {
        var work = new Work { UserId = userId };
        Apply(input, work);
        return work;
    }

    public static void Apply(WorkInput input, Work work)
    {
        work.Name = input.Name.Trim();
        work.ClientName = (input.ClientName ?? string.Empty).Trim();
        work.Location = (input.Location ?? string.Empty).Trim();
        work.Description = (input.Description ?? string.Empty).Trim();

        work.Items.Clear();

        var itemOrder = 0;
        foreach (var itemInput in input.Items)
        {
            var item = new WorkItem
            {
                Name = itemInput.Name.Trim(),
                Unit = string.IsNullOrWhiteSpace(itemInput.Unit) ? "un" : itemInput.Unit.Trim(),
                Quantity = itemInput.Quantity,
                WastePercent = Math.Clamp(itemInput.WastePercent, 0m, 100m),
                Notes = (itemInput.Notes ?? string.Empty).Trim(),
                SortOrder = itemOrder++
            };

            var materialOrder = 0;
            foreach (var materialInput in itemInput.Materials)
            {
                item.Materials.Add(new MaterialLine
                {
                    Description = materialInput.Description.Trim(),
                    Unit = string.IsNullOrWhiteSpace(materialInput.Unit) ? "un" : materialInput.Unit.Trim(),
                    QuantityPerUnit = materialInput.QuantityPerUnit,
                    UnitPrice = materialInput.UnitPrice,
                    SortOrder = materialOrder++
                });
            }

            var laborOrder = 0;
            foreach (var laborInput in itemInput.Labors)
            {
                item.Labors.Add(new LaborLine
                {
                    Description = laborInput.Description.Trim(),
                    Unit = string.IsNullOrWhiteSpace(laborInput.Unit) ? "h" : laborInput.Unit.Trim(),
                    QuantityPerUnit = laborInput.QuantityPerUnit,
                    UnitRate = laborInput.UnitRate,
                    SortOrder = laborOrder++
                });
            }

            work.Items.Add(item);
        }

        work.UpdatedAt = DateTime.UtcNow;

        var calculation = PricingCalculator.Calculate(work);
        work.MaterialTotal = calculation.MaterialTotal;
        work.LaborTotal = calculation.LaborTotal;
        work.Total = calculation.Total;
    }

    public static WorkSummaryDto ToSummary(Work work) => new(
        work.Id,
        work.Name,
        work.ClientName,
        work.Location,
        work.Currency,
        work.Items.Count,
        work.MaterialTotal,
        work.LaborTotal,
        work.Total,
        work.UpdatedAt);

    public static WorkDetailDto ToDetail(Work work)
    {
        var calculation = PricingCalculator.Calculate(work);
        return new WorkDetailDto(
            work.Id,
            work.Name,
            work.ClientName,
            work.Location,
            work.Description,
            work.Currency,
            work.CreatedAt,
            work.UpdatedAt,
            calculation.Items,
            calculation.MaterialTotal,
            calculation.LaborTotal,
            calculation.Total,
            calculation.MaterialShare,
            calculation.LaborShare);
    }
}
