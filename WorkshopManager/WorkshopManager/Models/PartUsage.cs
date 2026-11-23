namespace WorkshopManager.Models
{
    public class PartUsage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public ServiceJob ServiceJob { get; set; } = null!;
    }
}
