using System.Net;
using System.Net.Http.Json;
using BatchLab.Console.Data;
using BatchLab.Console.DTOs.Jobs;

namespace BatchLab.Console.Clients;

public sealed class JobsApiClient
    : ApiClientBase, IJobsApiClient
{
    public JobsApiClient(HttpClient http)
        : base(http)
    {
    }

    protected override string RoutePrefix => ApiRoutes.Jobs;

    public async Task<JobAcceptedResponse> StartBulkInsertAsync(
        BulkProductImportRequest request,
        CancellationToken ct = default)
    {
        using var response = await Http.PostAsJsonAsync(
            $"{RoutePrefix}/bulk-insert-data",
            request,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content
                .ReadAsStringAsync(ct);

            throw new HttpRequestException(
                $"API error: {error}",
                null,
                response.StatusCode);
        }

        var result = await response.Content
            .ReadFromJsonAsync<JobAcceptedResponse>(ct);

        return result ?? throw new InvalidOperationException(
            "Invalid API response.");
    }

    public async Task<JobStatusResponse?> GetStatusAsync(
        string jobId,
        CancellationToken ct = default)
    {
        using var response = await Http.GetAsync(
            $"{RoutePrefix}/{Uri.EscapeDataString(jobId)}",
            ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<JobStatusResponse>(ct);
    }

    public async Task<JobAcceptedResponse> StartBulkPriceUpdateAsync(
    BulkPriceUpdateRequest request,
    CancellationToken ct = default)
    {
        using var response = await Http.PostAsJsonAsync(
            $"{RoutePrefix}/bulk-update-price",
            request,
            ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<JobAcceptedResponse>(ct);

        return result ?? throw new InvalidOperationException(
            "Invalid API response.");
    }

    public async Task<JobAcceptedResponse> StartChunkedBulkInsertAsync(
    BulkProductImportRequest request,
    bool simulateFailure = false,
    CancellationToken ct = default)
    {
        string url =
            $"{RoutePrefix}/bulk-insert-chunked" +
            $"?simulateFailure={simulateFailure.ToString().ToLowerInvariant()}";

        using var response = await Http.PostAsJsonAsync(
            url, request, ct);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"API error: {error}",
                null,
                response.StatusCode);
        }

        return await response.Content
            .ReadFromJsonAsync<JobAcceptedResponse>(ct)
            ?? throw new InvalidOperationException(
                "Invalid API response.");
    }

    public async Task<JobAcceptedResponse> StartChunkedPriceUpdateAsync(
    BulkPriceUpdateRequest request,
    bool simulateFailure = false,
    CancellationToken ct = default)
    {
        string url =
            $"{RoutePrefix}/bulk-update-price-chunked" +
            $"?simulateFailure={simulateFailure.ToString().ToLowerInvariant()}";

        using var response = await Http.PostAsJsonAsync(
            url, request, ct);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(ct);

            throw new HttpRequestException(
                $"API error: {error}",
                null,
                response.StatusCode);
        }

        return await response.Content
            .ReadFromJsonAsync<JobAcceptedResponse>(ct)
            ?? throw new InvalidOperationException(
                "Invalid API response.");
    }
}