
using BatchLab.Console.Clients.Abstracts;
using BatchLab.Console.Clients.Interfaces;
using BatchLab.Console.Clients.Results;
using BatchLab.Console.Data;
using BatchLab.Contracts.Products;
using System.Net;
using System.Net.Http.Json;

namespace BatchLab.Console.Clients;

public sealed class ProductsApiClient
    : ApiClientBase, IProductsApiClient
{
    public ProductsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    protected override string RoutePrefix => ApiRoutes.Products;

    public async Task<ProductResponse?> GetByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        using var response = await Http.GetAsync(
            GetResourceUrl(id), ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductResponse>(
                cancellationToken: ct);
    }

    public async Task<ApiCallResult> AdjustStockAsync(
        long id,
        int quantityChange,
        CancellationToken ct = default)
    {
        var request = new AdjustStockRequest(quantityChange);

        using var response = await Http.PostAsJsonAsync(
            $"{GetResourceUrl(id)}/{ApiRoutes.AdjustStock}",
            request,
            ct);

        string? errorBody = null;

        if (!response.IsSuccessStatusCode)
        {
            errorBody = await response.Content
                .ReadAsStringAsync(ct);
        }

        return new ApiCallResult(
            response.StatusCode,
            errorBody);
    }
}
