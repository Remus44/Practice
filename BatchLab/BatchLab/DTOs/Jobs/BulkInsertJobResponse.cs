namespace BatchLab.DTOs.Jobs;

public record BulkInsertJobRequest(
    Guid OperationId,
    int Count);