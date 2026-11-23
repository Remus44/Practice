namespace BatchLab.Models;

public class ProcessedChunk
{
    public Guid OperationId { get; set; }
    public int ChunkIndex { get; set; }
    public string Signature { get; set; } = "";
    public DateTime CompletedAtUtc { get; set; }
}