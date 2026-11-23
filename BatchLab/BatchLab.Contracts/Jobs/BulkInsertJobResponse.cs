namespace BatchLab.Contracts.Jobs;

public record BulkInsertJobRequest(
    Guid OperationId,
    int Count);