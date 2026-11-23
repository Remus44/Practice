using BatchLab.Contracts.Jobs;

namespace BatchLab.Console.Clients.Interfaces;

public interface IJobsApiClient
{
    Task<JobAcceptedResponse> StartBulkInsertAsync(
        BulkProductImportRequest request,
        CancellationToken ct = default);

    Task<JobStatusResponse?> GetStatusAsync(
        string jobId,
        CancellationToken ct = default);

    Task<JobAcceptedResponse> StartBulkPriceUpdateAsync(
    BulkPriceUpdateRequest request,
    CancellationToken ct = default);

    Task<JobAcceptedResponse> StartChunkedBulkInsertAsync(
    BulkProductImportRequest request,
    bool simulateFailure = false,
    CancellationToken ct = default);

    Task<JobAcceptedResponse> StartChunkedPriceUpdateAsync(
    BulkPriceUpdateRequest request,
    bool simulateFailure = false,
    CancellationToken ct = default);
}