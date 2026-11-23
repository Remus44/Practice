namespace BatchLab.Models;

public class JobExecution
{
    public Guid OperationId { get; set; }
    public string OperationType { get; set; } = "";
    public string Parameters { get; set; } = "";
    public DateTime CompletedAtUtc { get; set; }
}