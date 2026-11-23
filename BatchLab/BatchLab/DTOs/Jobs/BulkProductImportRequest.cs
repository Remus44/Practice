namespace BatchLab.DTOs.Jobs;

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