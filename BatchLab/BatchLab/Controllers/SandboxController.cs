using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BatchLab.Models;
using BatchLab.Data;
using BatchLab.DTOs.Products;

namespace BatchLab.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SandboxController : ControllerBase
{
    private readonly BatchLabDbContext _context;
    public SandboxController(BatchLabDbContext context)
    {
        _context = context;
    }

    // GET: api/Product
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        return await _context.Products.ToListAsync();
    }

    // GET: api/Product/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProductById(long id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    // PUT: api/Product/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:long}")]
    public async Task<IActionResult> PutProduct(
    long id,
    UpdateProductRequest request,
    CancellationToken ct)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.Sku) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            request.Price < 0 ||
            request.StockQuantity < 0)
            return BadRequest("Invalid product data.");

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return NoContent();
    }

    // POST: api/Product
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetProductById", new { id = product.Id }, product);
    }

    // DELETE: api/Product/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(long? id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductExists(long? id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
