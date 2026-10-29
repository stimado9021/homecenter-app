namespace ManConstruc.Api.Models;

public class MaterialLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkItemId { get; set; }
    public WorkItem? WorkItem { get; set; }

    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = "un";
    public decimal QuantityPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public int SortOrder { get; set; }
}
