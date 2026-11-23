namespace BatchLab.Contracts.Jobs;

public record BulkPriceUpdateRequest(
    Guid OperationId,
    List<long> ProductIds,
    decimal PriceDelta
);