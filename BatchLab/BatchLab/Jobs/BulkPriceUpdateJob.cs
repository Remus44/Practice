using BatchLab.Data;
using BatchLab.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace BatchLab.Jobs;

public class BulkPriceUpdateJob
{
    private readonly BatchLabDbContext _db;
    private readonly ILogger<BulkPriceUpdateJob> _logger;
    private static readonly ConcurrentDictionary<Guid, byte>
    SimulatedFailures = new();

    public BulkPriceUpdateJob(
        BatchLabDbContext db,
        ILogger<BulkPriceUpdateJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task RunAsync(
        Guid operationId,
        long[] productIds,
        decimal priceDelta)
    {
        // 1. Bemenet ellenőrzése
        if (operationId == Guid.Empty ||
            productIds is null ||
            productIds.Length is < 1 or > 100 ||
            productIds.Any(id => id <= 0) ||
            priceDelta == 0 ||
            decimal.Round(priceDelta, 2) != priceDelta)
        {
            throw new ArgumentException("Invalid update request.");
        }

        // A sorrend és duplikációk normalizálása
        var ids = productIds
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        // 2. Egyedi aláírás a művelet tartalmából
        string input =
            $"{string.Join(",", ids)}|" +
            priceDelta.ToString("F2", CultureInfo.InvariantCulture);

        string hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(input)));

        string signature = $"bulk-price-update:{hash}";

        // 3. Közös tranzakció
        await using var tx =
            await _db.Database.BeginTransactionAsync();

        // 4. Korábbi végrehajtás ellenőrzése
        var previous = await _db.JobExecutions
            .AsNoTracking()
            .FirstOrDefaultAsync(j =>
                j.OperationId == operationId);

        if (previous is not null)
        {
            if (previous.OperationType != "BulkPriceUpdate" ||
                previous.Parameters != signature)
            {
                throw new InvalidOperationException(
                    "OperationId reused with different data.");
            }

            _logger.LogInformation(
                "Operation {Id} already completed.",
                operationId);

            return;
        }

        // 5. Idempotenciajelölő beszúrása
        _db.JobExecutions.Add(new JobExecution
        {
            OperationId = operationId,
            OperationType = "BulkPriceUpdate",
            Parameters = signature,
            CompletedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // 6. Valódi BULK UPDATE
        var now = DateTime.UtcNow;

        int affected = await _db.Products
            .Where(p => ids.Contains(p.Id) &&
                        p.Price + priceDelta >= 0)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    p => p.Price,
                    p => p.Price + priceDelta)
                .SetProperty(
                    p => p.Version,
                    p => p.Version + 1)
                .SetProperty(
                    p => p.UpdatedAt,
                    now));

        // Minden kiválasztott terméknek módosulnia kell
        if (affected != ids.Length)
        {
            throw new InvalidOperationException(
                $"Expected {ids.Length} updates, got {affected}.");
        }

        // 7. Csak teljes siker esetén commit
        await tx.CommitAsync();

        _logger.LogInformation(
            "Updated {Count} products.",
            affected);
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task RunChunkedAsync(
    Guid operationId,
    long[] productIds,
    decimal priceDelta,
    bool simulateFailure)
    {
        // 1. Validáció
        if (operationId == Guid.Empty ||
            productIds is null ||
            productIds.Length is < 1 or > 100 ||
            productIds.Any(id => id <= 0) ||
            priceDelta == 0 ||
            decimal.Round(priceDelta, 2) != priceDelta)
        {
            throw new ArgumentException("Invalid bulk update request.");
        }

        // 2. Az ID-k normalizálása
        var ids = productIds
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        // 3. Idempotencia-aláírás
        string input =
            $"{string.Join(",", ids)}|" +
            priceDelta.ToString("F2", CultureInfo.InvariantCulture);

        string hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(input)));

        string signature = $"price-update-chunked:{hash}";

        // 4. A teljes művelet már befejeződött?
        var completed = await _db.JobExecutions
            .AsNoTracking()
            .SingleOrDefaultAsync(j =>
                j.OperationId == operationId);

        if (completed is not null)
        {
            if (completed.OperationType != "ChunkedPriceUpdate" ||
                completed.Parameters != signature)
            {
                throw new InvalidOperationException(
                    "OperationId reused with different data.");
            }

            _logger.LogInformation(
                "Operation {Id} already completed.", operationId);

            return;
        }

        // 5. Két termék chunkonként, gyakorlási célból
        var chunks = ids.Chunk(2).ToArray();

        for (int index = 0; index < chunks.Length; index++)
        {
            await using var tx =
                await _db.Database.BeginTransactionAsync();

            // 6. Megnézzük, feldolgoztuk-e már ezt a chunkot
            var previous = await _db.ProcessedChunks
                .AsNoTracking()
                .SingleOrDefaultAsync(c =>
                    c.OperationId == operationId &&
                    c.ChunkIndex == index);

            if (previous is not null)
            {
                if (previous.Signature != signature)
                {
                    throw new InvalidOperationException(
                        "Chunk signature mismatch.");
                }

                _logger.LogInformation(
                    "Chunk {Index} already processed. Skipping.",
                    index);

                continue;
            }

            // 7. Szándékos hiba a harmadik chunknál,
            //    kizárólag az első próbálkozáskor
            if (simulateFailure &&
                index == 2 &&
                SimulatedFailures.TryAdd(operationId, 0))
            {
                throw new InvalidOperationException(
                    "Simulated failure at chunk 3.");
            }

            // 8. Chunk lefoglalása az összetett PK-val
            _db.ProcessedChunks.Add(new ProcessedChunk
            {
                OperationId = operationId,
                ChunkIndex = index,
                Signature = signature,
                CompletedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // 9. Halmazalapú UPDATE, nincs ToListAsync()
            var chunkIds = chunks[index];
            var now = DateTime.UtcNow;

            int affected = await _db.Products
                .Where(p =>
                    chunkIds.Contains(p.Id) &&
                    p.Price + priceDelta >= 0)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(
                        p => p.Price,
                        p => p.Price + priceDelta)
                    .SetProperty(
                        p => p.Version,
                        p => p.Version + 1)
                    .SetProperty(
                        p => p.UpdatedAt,
                        now));

            // 10. Minden kiválasztott termék frissült?
            if (affected != chunkIds.Length)
            {
                throw new InvalidOperationException(
                    $"Expected {chunkIds.Length} updates, got {affected}.");
            }

            // 11. A jelölő és az UPDATE közös commitja
            await tx.CommitAsync();

            _db.ChangeTracker.Clear();

            _logger.LogInformation(
                "Chunk {Index} committed. Updated {Count} products.",
                index, affected);
        }

        // 12. Az összes chunk elkészült
        _db.JobExecutions.Add(new JobExecution
        {
            OperationId = operationId,
            OperationType = "ChunkedPriceUpdate",
            Parameters = signature,
            CompletedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Chunked bulk update completed: {Count} products.",
            ids.Length);
    }
}