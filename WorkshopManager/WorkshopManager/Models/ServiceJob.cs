namespace WorkshopManager.Models
{
    public class ServiceJob
    {
        public string Id { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public ICollection<PartUsage> PartUsages { get; set; } = null!;
    }
}
