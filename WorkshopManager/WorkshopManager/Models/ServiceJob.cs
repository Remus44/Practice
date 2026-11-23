namespace WorkshopManager.Models
{
    public class ServiceJob
    {
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public Vehicle Vehicle { get; set; } = null!;


        public string Description { get; set; } = null!;

        public enum JobStatus
        {
            Open, InProgress, Completed
        }

        public JobStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public decimal LaborCost { get; set; }

        public ICollection<PartUsage> PartUsages { get; set; } = null!;
    }
}
