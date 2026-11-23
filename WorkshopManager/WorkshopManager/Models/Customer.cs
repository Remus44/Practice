namespace WorkshopManager.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = null!;
        
        public string Email { get; set; } = null!;

        public string? Phone {  get; set; }

        public ICollection<Vehicle> Vehicles
        { get; set; } = new List<Vehicle>();
    }
}
