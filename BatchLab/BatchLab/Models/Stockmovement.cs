using BatchLab.Models;

public class StockMovement
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public int QuantityChange { get; set; }

    public Product Product { get; set; } = null!;
}