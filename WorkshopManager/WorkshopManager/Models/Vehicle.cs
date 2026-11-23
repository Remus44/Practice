namespace WorkshopManager.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string LicencePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public Customer Customer { get; set; } = null!;
        public ICollection<ServiceJob> ServiceJobs { get; set; } = null!;
    }
}
