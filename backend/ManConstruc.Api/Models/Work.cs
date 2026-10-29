namespace ManConstruc.Api.Models;

public class Work
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Currency { get; set; } = "COP";

    public decimal MaterialTotal { get; set; }
    public decimal LaborTotal { get; set; }
    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<WorkItem> Items { get; set; } = new();
}
