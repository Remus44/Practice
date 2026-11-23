using BatchLab.DTOs.Jobs;
using BatchLab.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace BatchLab.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IBackgroundJobClient _jobs;
    private readonly JobStorage _storage;

    public JobsController(
        IBackgroundJobClient jobs,
        JobStorage storage)
    {
        _jobs = jobs;
        _storage = storage;
    }

    [HttpPost("demo")]
    public IActionResult StartDemoJob()
    {
        string jobId = _jobs.Enqueue<DemoJob>(
            job => job.RunAsync());

        return Accepted(new { jobId });
    }

    [HttpGet("{jobId}")]
    public IActionResult GetStatus(string jobId)
    {
        using var connection = _storage.GetConnection();

        var job = connection.GetJobData(jobId);

        if (job is null)
            return NotFound();

        return Ok(new
        {
            JobId = jobId,
            Status = job.State
        });
    }

    [HttpPost("demo-fail")]
    public IActionResult StartFailingDemoJob()
    {
        string jobId = _jobs.Enqueue<DemoJob>(
            job => job.FailAsync());

        return Accepted(new { jobId });
    }

    [HttpPost("bulk-insert")]
    public IActionResult StartBulkInsert(
    BulkInsertJobRequest request)
    {
        if (request.OperationId == Guid.Empty ||
            request.Count is < 1 or > 1000)
        {
            return BadRequest("Invalid arguments.");
        }

        var operationId = request.OperationId;
        var count = request.Count;

        string jobId = _jobs.Enqueue<BulkInsertJob>(
            job => job.RunAsync(operationId, count));

        return Accepted(new
        {
            jobId,
            operationId
        });
    }

    [HttpPost("bulk-insert-data")]
    public IActionResult StartBulkInsertData(
    [FromBody] BulkProductImportRequest request)
    {
        if (request.OperationId == Guid.Empty ||
            request.Products is not { Count: >= 1 and <= 100 })
        {
            return BadRequest("Invalid import request.");
        }

        // Egyszerű üzleti validáció
        if (request.Products.Any(p =>
            string.IsNullOrWhiteSpace(p.Sku) ||
            p.Sku.Length > 64 ||
            string.IsNullOrWhiteSpace(p.Name) ||
            p.Name.Length > 200 ||
            p.Price < 0 ||
            p.StockQuantity < 0))
        {
            return BadRequest("Invalid product data.");
        }

        var operationId = request.OperationId;
        var products = request.Products.ToArray();

        string jobId = _jobs.Enqueue<BulkInsertJob>(
            job => job.RunFromClientAsync(operationId, products));

        return Accepted(new
        {
            jobId,
            operationId
        });
    }

    [HttpPost("bulk-update-price")]
    public IActionResult StartBulkPriceUpdate(
    [FromBody] BulkPriceUpdateRequest request)
    {
        if (request.OperationId == Guid.Empty ||
            request.ProductIds is not { Count: >= 1 and <= 100 } ||
            request.ProductIds.Any(id => id <= 0) ||
            request.PriceDelta == 0 ||
            decimal.Round(request.PriceDelta, 2) != request.PriceDelta)
        {
            return BadRequest("Invalid update request.");
        }

        var operationId = request.OperationId;
        var ids = request.ProductIds
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        var delta = request.PriceDelta;

        string jobId = _jobs.Enqueue<BulkPriceUpdateJob>(
            job => job.RunAsync(operationId, ids, delta));

        return Accepted(new
        {
            jobId,
            operationId,
            productCount = ids.Length
        });
    }

    [HttpPost("bulk-insert-chunked")]
    public IActionResult StartChunkedImport(
    [FromBody] BulkProductImportRequest request,
    [FromQuery] bool simulateFailure = false)
    {
        if (request.OperationId == Guid.Empty ||
            request.Products is not { Count: >= 1 and <= 100 })
            return BadRequest("Invalid import request.");

        if (request.Products.Any(p =>
            string.IsNullOrWhiteSpace(p.Sku) ||
            p.Sku.Length > 64 ||
            string.IsNullOrWhiteSpace(p.Name) ||
            p.Name.Length > 200 ||
            p.Price < 0 ||
            p.StockQuantity < 0))
            return BadRequest("Invalid product data.");

        var operationId = request.OperationId;
        var products = request.Products.ToArray();

        string jobId = _jobs.Enqueue<BulkInsertJob>(
            job => job.RunChunkedAsync(
                operationId, products, simulateFailure));

        return Accepted(new { jobId, operationId });
    }

    [HttpPost("bulk-update-price-chunked")]
    public IActionResult StartChunkedPriceUpdate(
    [FromBody] BulkPriceUpdateRequest request,
    [FromQuery] bool simulateFailure = false)
    {
        if (request.OperationId == Guid.Empty ||
            request.ProductIds is not { Count: >= 1 and <= 100 } ||
            request.ProductIds.Any(id => id <= 0) ||
            request.PriceDelta == 0 ||
            decimal.Round(request.PriceDelta, 2) != request.PriceDelta)
        {
            return BadRequest("Invalid update request.");
        }

        var operationId = request.OperationId;

        var ids = request.ProductIds
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        var delta = request.PriceDelta;
        var fail = simulateFailure;

        string jobId = _jobs.Enqueue<BulkPriceUpdateJob>(
            job => job.RunChunkedAsync(
                operationId, ids, delta, fail));

        return Accepted(new
        {
            jobId,
            operationId,
            productCount = ids.Length
        });
    }
}