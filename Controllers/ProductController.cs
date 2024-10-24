// Controllers/ProductController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt1.Models;

// Controllers/ProductController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/product
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    // GET: api/product/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    // POST: api/product
    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        if (await _context.Products.AnyAsync(p => p.Name == product.Name))
        {
            ModelState.AddModelError("Name", "Nazwa produktu musi być unikalna.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Dodaj do historii logów
        product.ChangeHistory.Add(new ChangeLog { ChangeDescription = "Produkt został utworzony." });

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // PUT: api/product/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest("Identyfikator produktu nie jest zgodny.");
        }

        if (await _context.Products.AnyAsync(p => p.Name == product.Name && p.Id != id))
        {
            ModelState.AddModelError("Name", "Nazwa produktu musi być unikalna.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        // Sprawdź, czy stan magazynowy uległ zmianie
        if (existingProduct.StockQuantity != product.StockQuantity)
        {
            string description = product.StockQuantity == 0 ?
                "Stan magazynowy osiągnął 0, produkt niedostępny." :
                $"Zaktualizowano ilość do {product.StockQuantity}.";

            product.ChangeHistory.Add(new ChangeLog { ChangeDescription = description });
        }

        _context.Entry(existingProduct).CurrentValues.SetValues(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/product/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
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

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }

    // Wyświetlanie historii zmian
    [HttpGet("{id}/history")]
    public async Task<ActionResult<IEnumerable<ChangeLog>>> GetProductHistory(int id)
    {
        var product = await _context.Products.Include(p => p.ChangeHistory).FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product.ChangeHistory);
    }

}