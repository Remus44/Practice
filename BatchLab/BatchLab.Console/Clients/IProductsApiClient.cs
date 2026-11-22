
using BatchLab.Console.DTOs;

namespace BatchLab.Console.Clients;

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
