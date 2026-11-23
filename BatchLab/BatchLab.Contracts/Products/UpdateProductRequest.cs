namespace BatchLab.Contracts.Products;

public record UpdateProductRequest(
    string Sku,
    string Name,
    decimal Price,
    int StockQuantity);
