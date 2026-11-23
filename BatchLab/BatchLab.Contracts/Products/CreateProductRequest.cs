namespace BatchLab.Contracts.Products;

public record CreateProductRequest(
string Sku,
string Name,
decimal Price,
int StockQuantity);

