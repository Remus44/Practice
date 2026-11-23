using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using BatchLab.Contracts.Jobs;
using BatchLab.Data;
using BatchLab.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace BatchLab.Jobs;

public class BulkInsertJob
{
    private readonly BatchLabDbContext _db;
    private readonly ILogger<BulkInsertJob> _logger;
    private static readonly ConcurrentDictionary<Guid, byte> SimulatedFailures = new();

    public BulkInsertJob(
        BatchLabDbContext db,
        ILogger<BulkInsertJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task RunAsync(Guid operationId, int count)
    {
        if (operationId == Guid.Empty ||
            count is < 1 or > 1000)
        {
            throw new ArgumentException("Invalid job arguments.");
        }

        string signature = $"bulk-insert:{count}";

        await using var tx =
            await _db.Database.BeginTransactionAsync();

        // 1. Ellenőrizzük, hogy ez a művelet már lefutott-e.
        var previous = await _db.JobExecutions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                j => j.OperationId == operationId);

        if (previous is not null)
        {
            if (previous.Parameters != signature)
            {
                throw new InvalidOperationException(
                    "OperationId reused with different arguments.");
            }

            _logger.LogInformation(
                "Operation {Id} already completed. Skipping.",
                operationId);

            return;
        }

        // 2. Lefoglaljuk az üzleti művelet azonosítóját.
        _db.JobExecutions.Add(new JobExecution
        {
            OperationId = operationId,
            OperationType = "BulkInsert",
            Parameters = signature,
            CompletedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // 3. Elkészítjük a termékeket.
        var products = Enumerable.Range(1, count)
            .Select(i => new Product
            {
                Sku = $"HF-{operationId:N}-{i:D4}",
                Name = $"Hangfire Product {i}",
                Price = 1000m + i,
                StockQuantity = 25,
                UpdatedAt = DateTime.UtcNow
            })
            .ToList();

        // 4. Tömeges beszúrás.
        _db.Products.AddRange(products);

        await _db.SaveChangesAsync();

        // 5. A jelölő és a termékek együtt commitolódnak.
        await tx.CommitAsync();

        _logger.LogInformation(
            "Bulk insert completed: {Count} products.",
            count);
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task RunFromClientAsync(
    Guid operationId,
    ProductImportItem[] items)
    {
        if (operationId == Guid.Empty ||
            items is not { Length: >= 1 and <= 100 })
        {
            throw new ArgumentException("Invalid import.");
        }

        // 1. A teljes bemenetből hash készül.
        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(items);
        string hash = Convert.ToHexString(
            SHA256.HashData(jsonBytes));

        string signature = $"bulk-insert-data:{hash}";

        // 2. Tranzakció indítása.
        await using var tx =
            await _db.Database.BeginTransactionAsync();

        // 3. Ellenőrizzük, hogy a művelet már lefutott-e.
        var previous = await _db.JobExecutions
            .AsNoTracking()
            .SingleOrDefaultAsync(j =>
                j.OperationId == operationId);

        if (previous is not null)
        {
            if (previous.OperationType != "BulkInsertClient" ||
                previous.Parameters != signature)
            {
                throw new InvalidOperationException(
                    "OperationId already used with different data.");
            }

            _logger.LogInformation(
                "Operation {OperationId} already completed. Skipping.",
                operationId);

            return;
        }

        // 4. Idempotenciajelölő létrehozása.
        _db.JobExecutions.Add(new JobExecution
        {
            OperationId = operationId,
            OperationType = "BulkInsertClient",
            Parameters = signature,
            CompletedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // 5. A kliensadatok átalakítása EF entitásokká.
        var now = DateTime.UtcNow;

        var products = items.Select(item => new Product
        {
            Sku = item.Sku,
            Name = item.Name,
            Price = item.Price,
            StockQuantity = item.StockQuantity,
            UpdatedAt = now
        }).ToList();

        // 6. Bulk insert.
        _db.Products.AddRange(products);

        await _db.SaveChangesAsync();

        // 7. Közös commit.
        await tx.CommitAsync();

        _logger.LogInformation(
            "Imported {Count} products. Operation: {OperationId}",
            products.Count,
            operationId);
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task RunChunkedAsync(
    Guid operationId,
    ProductImportItem[] items,
    bool simulateFailure)
    {
        if (operationId == Guid.Empty ||
            items is not { Length: >= 1 and <= 100 })
            throw new ArgumentException("Invalid import.");

        string hash = Convert.ToHexString(
            SHA256.HashData(
                JsonSerializer.SerializeToUtf8Bytes(items)));

        string signature = $"bulk-insert-chunked:{hash}";

        // Már teljesen befejezett művelet?
        var completed = await _db.JobExecutions
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.OperationId == operationId);

        if (completed is not null)
        {
            if (completed.OperationType != "ChunkedBulkInsert" ||
                completed.Parameters != signature)
                throw new InvalidOperationException(
                    "OperationId reused with different data.");

            _logger.LogInformation("Already completed.");
            return;
        }

        // Most 2-es chunkméret, hogy 6 termékkel tesztelhessünk.
        var chunks = items.Chunk(2).ToArray();

        for (int index = 0; index < chunks.Length; index++)
        {
            await using var tx =
                await _db.Database.BeginTransactionAsync();

            var previous = await _db.ProcessedChunks
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.OperationId == operationId &&
                    x.ChunkIndex == index);

            if (previous is not null)
            {
                if (previous.Signature != signature)
                    throw new InvalidOperationException(
                        "OperationId reused with different data.");

                _logger.LogInformation(
                    "Chunk {Index} already done. Skipping.", index);

                continue;
            }

            // Kizárólag teszt: a 3. chunk első futásakor hiba.
            if (simulateFailure &&
                index == 2 &&
                SimulatedFailures.TryAdd(operationId, 0))
            {
                throw new InvalidOperationException(
                    "Simulated failure at chunk 3.");
            }

            // Jelölő és termékek ugyanabban a tranzakcióban.
            _db.ProcessedChunks.Add(new ProcessedChunk
            {
                OperationId = operationId,
                ChunkIndex = index,
                Signature = signature,
                CompletedAtUtc = DateTime.UtcNow
            });

            _db.Products.AddRange(
                chunks[index].Select(item => new Product
                {
                    Sku = item.Sku,
                    Name = item.Name,
                    Price = item.Price,
                    StockQuantity = item.StockQuantity,
                    UpdatedAt = DateTime.UtcNow
                }));

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _db.ChangeTracker.Clear();

            _logger.LogInformation(
                "Chunk {Index} committed.", index);
        }

        // Minden chunk készen van.
        _db.JobExecutions.Add(new JobExecution
        {
            OperationId = operationId,
            OperationType = "ChunkedBulkInsert",
            Parameters = signature,
            CompletedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Chunked import completed: {Count} products.",
            items.Length);
    }
}