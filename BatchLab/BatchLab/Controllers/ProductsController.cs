
using BatchLab.Contracts.Products;
using BatchLab.Data;
using BatchLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BatchLab.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly BatchLabDbContext _db;

    public ProductsController(BatchLabDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAll(
        CancellationToken ct)
    {
        var products = await _db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new ProductResponse(
                p.Id, p.Sku, p.Name, p.Price,
                p.StockQuantity, p.UpdatedAt))
            .ToListAsync(ct);

        return Ok(products);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductResponse>> GetById(
        long id, CancellationToken ct)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponse(
                p.Id, p.Sku, p.Name, p.Price,
                p.StockQuantity, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            request.Price < 0 ||
            request.StockQuantity < 0)
        {
            return BadRequest("Invalid product data.");
        }

        if (await _db.Products.AnyAsync(
            p => p.Sku == request.Sku, ct))
        {
            return Conflict("SKU already exists.");
        }

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        var response = new ProductResponse(
            product.Id, product.Sku, product.Name,
            product.Price, product.StockQuantity,
            product.UpdatedAt);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            response);
    }

    [HttpPost("{id:long}/adjust-stock")]
    public async Task<IActionResult> AdjustStock(
    long id,
    AdjustStockRequest request,
    CancellationToken ct)
    {
        if (request.QuantityChange == 0)
        {
            return BadRequest("Quantity change cannot be zero.");
        }

        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return NotFound();

        long newQuantity =
            (long)product.StockQuantity + request.QuantityChange;

        if (newQuantity < 0 || newQuantity > int.MaxValue)
            return Conflict("Invalid resulting stock quantity.");

        int previousQuantity = product.StockQuantity;

        product.StockQuantity = (int)newQuantity;
        product.Version++;
        product.UpdatedAt = DateTime.UtcNow;

        var movement = new StockMovement
        {
            ProductId = product.Id,
            QuantityChange = request.QuantityChange
        };

        _db.StockMovements.Add(movement);

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            ProductId = product.Id,
            PreviousQuantity = previousQuantity,
            NewQuantity = product.StockQuantity,
            QuantityChange = request.QuantityChange
        });
    }

    [HttpPost("{id:long}/adjust-stock-tx")]
    public async Task<IActionResult> AdjustStockTransactional(
    long id,
    AdjustStockRequest request,
    CancellationToken ct,
    [FromQuery] bool simulateFailure = false)
    {
        if (request.QuantityChange == 0)
            return BadRequest("Quantity change cannot be zero.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (product is null)
                return NotFound();

            long newQuantity =
                (long)product.StockQuantity + request.QuantityChange;

            if (newQuantity < 0 || newQuantity > int.MaxValue)
                return Conflict("Invalid resulting stock quantity.");

            int previousQuantity = product.StockQuantity;

            // 1. Első adatbázis-módosítás
            product.StockQuantity = (int)newQuantity;
            product.Version++;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            // 2. Második adatbázis-módosítás
            _db.StockMovements.Add(new StockMovement
            {
                ProductId = product.Id,
                QuantityChange = request.QuantityChange
            });

            await _db.SaveChangesAsync(ct);

            // 3. Szimulált hiba a két mentés után
            if (simulateFailure)
            {
                await transaction.RollbackAsync(ct);
                return StatusCode(500, "Simulated failure: rolled back.");
            }

            // 4. Mindkét módosítás véglegesítése
            await transaction.CommitAsync(ct);

            return Ok(new
            {
                ProductId = product.Id,
                PreviousQuantity = previousQuantity,
                NewQuantity = product.StockQuantity
            });
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    [HttpPost("{id:long}/adjust-stock-unsafe")]
    public async Task<IActionResult> AdjustStockUnsafe(
    long id,
    AdjustStockRequest request,
    CancellationToken ct)
    {
        if (request.QuantityChange == 0)
            return BadRequest("Quantity change cannot be zero.");

        // 1. Betöltjük a terméket
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return NotFound();

        int previousQuantity = product.StockQuantity;

        // 2. Mesterséges késleltetés
        // Hogy két kérés egyszerre dolgozhasson
        await Task.Delay(5000, ct);

        // 3. Készletmódosítás
        long newQuantity =
            (long)previousQuantity + request.QuantityChange;

        if (newQuantity < 0 || newQuantity > int.MaxValue)
            return Conflict("Invalid stock quantity.");

        product.StockQuantity = (int)newQuantity;
        product.Version++;
        product.UpdatedAt = DateTime.UtcNow;

        // 4. Készletmozgás
        _db.StockMovements.Add(new StockMovement
        {
            ProductId = id,
            QuantityChange = request.QuantityChange
        });

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            PreviousQuantity = previousQuantity,
            NewQuantity = product.StockQuantity
        });
    }


    [HttpPost("{id:long}/adjust-stock-locked")]
    public async Task<IActionResult> AdjustStockLocked(
        long id,
        AdjustStockRequest request,
        CancellationToken ct)
    {
        if (request.QuantityChange == 0)
            return BadRequest("Quantity change cannot be zero.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Zárolva olvassuk a terméket
            var products = await _db.Products
                .FromSqlRaw(
                    SqlQueries.SelectProductForUpdate, id)
                .ToListAsync(ct);

            var product = products.SingleOrDefault();

            if (product is null)
                return NotFound();

            // 2. Teszt: 5 másodpercig tartjuk a lockot
            await Task.Delay(5000, ct);

            long newQuantity =
                (long)product.StockQuantity + request.QuantityChange;

            if (newQuantity < 0 || newQuantity > int.MaxValue)
                return Conflict("Invalid resulting stock quantity.");

            int previousQuantity = product.StockQuantity;

            // 3. Készlet módosítása
            product.StockQuantity = (int)newQuantity;
            product.Version++;
            product.UpdatedAt = DateTime.UtcNow;

            // 4. Készletmozgás rögzítése
            _db.StockMovements.Add(new StockMovement
            {
                ProductId = product.Id,
                QuantityChange = request.QuantityChange
            });

            await _db.SaveChangesAsync(ct);

            // 5. Tranzakció véglegesítése
            await transaction.CommitAsync(ct);

            return Ok(new
            {
                ProductId = product.Id,
                PreviousQuantity = previousQuantity,
                NewQuantity = product.StockQuantity
            });
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);
            throw;
        }
    }


    [HttpPost("{id:long}/adjust-stock-atomic")]
    public async Task<IActionResult> AdjustStockAtomic(
        long id,
        AdjustStockRequest request,
        CancellationToken ct,
        [FromQuery] bool simulateDelay = false)
    {
        int delta = request.QuantityChange;

        if (delta == 0)
            return BadRequest("Quantity change cannot be zero.");

        // A legkisebb int negatív értékének
        // abszolút értéke már nem férne el int-ben.
        if (delta == int.MinValue)
            return Conflict("Invalid quantity change.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Feltételes, atomikus UPDATE
            var query = _db.Products
                .Where(p => p.Id == id && p.StockQuantity >= 0);

            if (delta < 0)
            {
                query = query.Where(
                    p => p.StockQuantity >= -delta);
            }
            else
            {
                query = query.Where(
                    p => p.StockQuantity <= int.MaxValue - delta);
            }

            var now = DateTime.UtcNow;

            int affectedRows = await query.ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        p => p.StockQuantity,
                        p => p.StockQuantity + delta)
                    .SetProperty(
                        p => p.Version,
                        p => p.Version + 1)
                    .SetProperty(
                        p => p.UpdatedAt,
                        now),
                ct);

            // 2. Ellenőrizzük, sikerült-e a módosítás
            if (affectedRows == 0)
            {
                bool exists = await _db.Products
                    .AnyAsync(p => p.Id == id, ct);

                return exists
                    ? Conflict("Invalid resulting stock quantity.")
                    : NotFound();
            }

            // Kizárólag a konkurenciateszthez.
            if (simulateDelay)
                await Task.Delay(3000, ct);

            // 3. Készletmozgás rögzítése
            _db.StockMovements.Add(new StockMovement
            {
                ProductId = id,
                QuantityChange = delta
            });

            await _db.SaveChangesAsync(ct);

            // 4. Mindkét művelet véglegesítése
            await transaction.CommitAsync(ct);

            return Ok(new
            {
                ProductId = id,
                QuantityChange = delta,
                UpdatedRows = affectedRows
            });
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);
            throw;
        }
    }


    [HttpPost("{id:long}/adjust-stock-optimistic")]
    public async Task<IActionResult> AdjustStockOptimistic(
        long id,
        AdjustStockRequest request,
        CancellationToken ct,
        [FromQuery] bool simulateDelay = false)
    {
        if (request.QuantityChange == 0)
            return BadRequest("Quantity change cannot be zero.");

        // 1. Termék lekérése - nincs explicit lock
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return NotFound();

        long newQuantity =
            (long)product.StockQuantity + request.QuantityChange;

        if (newQuantity < 0 || newQuantity > int.MaxValue)
            return Conflict("Invalid resulting stock quantity.");

        // 2. Versenyhelyzet szimulálása
        if (simulateDelay)
            await Task.Delay(3000, ct);

        // 3. Követett entitás módosítása
        product.StockQuantity = (int)newQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        // A verziót alkalmazásszinten növeljük
        product.Version++;

        _db.StockMovements.Add(new StockMovement
        {
            ProductId = product.Id,
            QuantityChange = request.QuantityChange
        });

        try
        {
            // 4. EF Core concurrency ellenőrzés
            // és atomikus mentés
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Valaki időközben módosította a rekordot
            return Conflict(new
            {
                Message = "Product was modified concurrently.",
                ProductId = id,
                Action = "Reload product and retry."
            });
        }

        return Ok(new
        {
            ProductId = product.Id,
            NewQuantity = product.StockQuantity,
            Version = product.Version
        });
    }


    [HttpPost("generate")]
    public async Task<IActionResult> GenerateProducts(
        [FromQuery] int count,
        CancellationToken ct)
    {
        if (count < 1 || count > 300)
            return BadRequest("Count must be between 1 and 300.");

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            var product = new Product
            {
                Sku = "GEN-" + Guid.NewGuid().ToString("N")[..16],
                Name = $"BulkTest-{i + 1}",
                Price = 1000m + i * 10m,
                StockQuantity = 10 + i % 90,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);

            // Szándékosan minden rekordnál mentünk!
            await _db.SaveChangesAsync(ct);
        }

        stopwatch.Stop();

        return Ok(new
        {
            Mode = "Naive",
            InsertedRows = count,
            SaveChangesCalls = count,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
        });
    }


    [HttpPost("generate-range")]
    public async Task<IActionResult> GenerateProductsRange(
        [FromQuery] int count,
        CancellationToken ct)
    {
        if (count < 1 || count > 300)
            return BadRequest("Count must be between 1 and 300.");

        var stopwatch = Stopwatch.StartNew();

        // 1. Előkészítjük a teljes adathalmazt
        var products = new List<Product>(count);

        for (int i = 0; i < count; i++)
        {
            products.Add(new Product
            {
                Sku = "GEN-" + Guid.NewGuid().ToString("N")[..16],
                Name = $"BulkTest-{i + 1}",
                Price = 1000m + i * 10m,
                StockQuantity = 10 + i % 90,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // 2. Az összes terméket felvesszük a ChangeTrackerbe
        _db.Products.AddRange(products);

        // 3. Egyetlen SaveChanges hívás
        await _db.SaveChangesAsync(ct);

        stopwatch.Stop();

        return Ok(new
        {
            Mode = "AddRange",
            InsertedRows = products.Count,
            SaveChangesCalls = 1,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
        });
    }


    [HttpPost("generate-batched")]
    public async Task<IActionResult> GenerateProductsBatched(
        [FromQuery] int count,
        CancellationToken ct,
        [FromQuery] int batchSize = 500,
        [FromQuery] bool clearTracker = true)
    {
        if (count < 1 || count > 100_000)
            return BadRequest("Count must be between 1 and 100000.");

        if (batchSize < 1 || batchSize > 5000)
            return BadRequest("Batch size must be between 1 and 5000.");

        var stopwatch = Stopwatch.StartNew();

        int inserted = 0;
        int batches = 0;
        int trackedBeforeLastClear = 0;

        while (inserted < count)
        {
            // 1. Meghatározzuk az aktuális batch méretét
            int currentBatchSize = Math.Min(
                batchSize,
                count - inserted);

            var products = new List<Product>(currentBatchSize);

            // 2. Létrehozzuk az aktuális batch rekordjait
            for (int i = 0; i < currentBatchSize; i++)
            {
                int index = inserted + i;

                products.Add(new Product
                {
                    Sku = "GEN-" + Guid.NewGuid().ToString("N")[..16],
                    Name = $"BatchTest-{index + 1}",
                    Price = 1000m + (index % 500) * 10m,
                    StockQuantity = 10 + index % 90,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // 3. A batch mentése
            _db.Products.AddRange(products);
            await _db.SaveChangesAsync(ct);

            inserted += currentBatchSize;
            batches++;

            // 4. Az utolsó batchnél megnézzük a trackinget
            if (inserted == count)
            {
                trackedBeforeLastClear = _db.ChangeTracker
                    .Entries<Product>()
                    .Count();
            }

            // 5. Felszabadítjuk a követett entitásokat
            if (clearTracker)
            {
                _db.ChangeTracker.Clear();
            }
        }

        stopwatch.Stop();

        int trackedAtEnd = _db.ChangeTracker
            .Entries<Product>()
            .Count();

        return Ok(new
        {
            Mode = "BatchedAddRange",
            InsertedRows = inserted,
            BatchSize = batchSize,
            Batches = batches,
            SaveChangesCalls = batches,
            ClearTracker = clearTracker,
            TrackedBeforeLastClear = trackedBeforeLastClear,
            TrackedAtEnd = trackedAtEnd,
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
        });
    }


    [HttpPut("bulk-update-tracked")]
    public async Task<IActionResult> BulkUpdateTracked(
        [FromQuery] long fromId,
        [FromQuery] long toId,
        CancellationToken ct)
    {
        if (fromId <= 0 || toId < fromId)
            return BadRequest("Invalid ID range.");

        var stopwatch = Stopwatch.StartNew();

        // 1. Betöltjük a módosítandó entitásokat
        var products = await _db.Products
            .Where(p => p.Id >= fromId
                     && p.Id <= toId
                     && p.Sku.StartsWith("GEN-"))
            .ToListAsync(ct);

        // 2. C#-ban módosítjuk őket
        var now = DateTime.UtcNow;

        foreach (var product in products)
        {
            product.Price += 1m;
            product.Version++;
            product.UpdatedAt = now;
        }

        // 3. Egyetlen SaveChanges hívás
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Concurrent modification detected.");
        }

        stopwatch.Stop();

        return Ok(new
        {
            Mode = "Tracked",
            UpdatedRows = products.Count,
            TrackedEntities = _db.ChangeTracker
                .Entries<Product>().Count(),
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
        });
    }


    [HttpPut("bulk-update-atomic")]
    public async Task<IActionResult> BulkUpdateAtomic(
        [FromQuery] long fromId,
        [FromQuery] long toId,
        CancellationToken ct)
    {
        if (fromId <= 0 || toId < fromId)
            return BadRequest("Invalid ID range.");

        var stopwatch = Stopwatch.StartNew();

        var now = DateTime.UtcNow;

        // Egyetlen SQL UPDATE
        int updatedRows = await _db.Products
            .Where(p => p.Id >= fromId
                     && p.Id <= toId
                     && p.Sku.StartsWith("GEN-"))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    p => p.Price,
                    p => p.Price + 1m)
                .SetProperty(
                    p => p.Version,
                    p => p.Version + 1)
                .SetProperty(
                    p => p.UpdatedAt,
                    now),
                ct);

        stopwatch.Stop();

        return Ok(new
        {
            Mode = "ExecuteUpdate",
            UpdatedRows = updatedRows,
            TrackedEntities = _db.ChangeTracker
                .Entries<Product>().Count(),
            ElapsedMs = stopwatch.Elapsed.TotalMilliseconds
        });
    }

}