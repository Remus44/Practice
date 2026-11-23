using BatchLab.Console.Clients.Results;
using BatchLab.Contracts.Products;

namespace BatchLab.Console.Clients.Interfaces;

public interface IProductsApiClient
{
    Task<ProductResponse?> GetByIdAsync(
        long id,
        CancellationToken ct = default);

    Task<ApiCallResult> AdjustStockAsync(
        long id,
        int quantityChange,
        CancellationToken ct = default);
}
