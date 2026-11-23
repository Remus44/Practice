namespace WorkshopManager.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        public string LicencePlate { get; set; } = null!;

        public string Brand { get; set; } = null!;

        public string Model { get; set; } = null!;

        public int Year { get; set; }

        public ICollection<ServiceJob> ServiceJobs { get; set; } = null!;
    }
}
