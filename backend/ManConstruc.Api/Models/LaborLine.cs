namespace ManConstruc.Api.Models;

public class LaborLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public WorkItem? WorkItem { get; set; }

    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = "h";
    public decimal QuantityPerUnit { get; set; }
    public decimal UnitRate { get; set; }
    public int SortOrder { get; set; }
}
