namespace BatchLab.Contracts.Products;
public record ProductResponse(
    long Id,
    string Sku,
    string Name,
    decimal Price,
    int StockQuantity,
    DateTime UpdatedAt);

