namespace ManConstruc.Api.Models;

public class WorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkId { get; set; }
    public Work? Work { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "un";
    public decimal Quantity { get; set; }
    public decimal WastePercent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public List<MaterialLine> Materials { get; set; } = new();
    public List<LaborLine> Labors { get; set; } = new();
}
