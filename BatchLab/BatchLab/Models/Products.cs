namespace BatchLab.Models;

public class Product
{
    public long Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long Version { get; set; }

    public ICollection<StockMovement> StockMovements
    { get; set; } = new List<StockMovement>();
}