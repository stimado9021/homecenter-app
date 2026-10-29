namespace ManConstruc.Api.Contracts;

public class WorkCalculation
{
    public List<ItemCalculation> Items { get; set; } = new();
    public decimal MaterialTotal { get; set; }
    public decimal LaborTotal { get; set; }
    public decimal Total { get; set; }
    public decimal MaterialShare { get; set; }
    public decimal LaborShare { get; set; }
    public int ItemCount => Items.Count;
}

public class ItemCalculation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal WastePercent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<MaterialCalculation> Materials { get; set; } = new();
    public List<LaborCalculation> Labors { get; set; } = new();
    public decimal MaterialTotal { get; set; }
    public decimal LaborTotal { get; set; }
    public decimal Total { get; set; }
    public decimal UnitTotal { get; set; }
}

public class MaterialCalculation
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal QuantityPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal WastePercent { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class LaborCalculation
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal QuantityPerUnit { get; set; }
    public decimal UnitRate { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal LineTotal { get; set; }
}
