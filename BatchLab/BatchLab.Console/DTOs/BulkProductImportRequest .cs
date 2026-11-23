namespace BatchLab.Console.DTOs.Jobs;

public record ProductImportItem(
    string Sku,
    string Name,
    decimal Price,
    int StockQuantity
);

public record BulkProductImportRequest(
    Guid OperationId,
    List<ProductImportItem> Products
);

public record JobAcceptedResponse(
    string JobId,
    Guid OperationId
);

public record JobStatusResponse(
    string JobId,
    string Status
);