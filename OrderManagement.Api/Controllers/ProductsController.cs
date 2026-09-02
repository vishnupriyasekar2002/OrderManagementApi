using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
	private readonly AppDbContext _context;

	public ProductsController(AppDbContext context)
	{
		_context = context;
	}

	// GET: api/products
	[HttpGet]
	public async Task<IActionResult> GetProducts()
	{
		var products = await _context.Products
			.AsNoTracking()
			.ToListAsync();

		return Ok(products);
	}

	// GET: api/products/1
	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetProduct(int id)
	{
		var product = await _context.Products
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == id);

		if (product == null)
		{
			return NotFound(new
			{
				message = "Product not found."
			});
		}

		return Ok(product);
	}

	// POST: api/products
	[HttpPost]
	public async Task<IActionResult> CreateProduct(Product product)
	{
		if (string.IsNullOrWhiteSpace(product.Name))
		{
			return BadRequest(new
			{
				message = "Product name is required."
			});
		}

		if (product.Price <= 0)
		{
			return BadRequest(new
			{
				message = "Price must be greater than zero."
			});
		}

		if (product.StockQuantity < 0)
		{
			return BadRequest(new
			{
				message = "Stock quantity cannot be negative."
			});
		}

		_context.Products.Add(product);

		await _context.SaveChangesAsync();

		return CreatedAtAction(
			nameof(GetProduct),
			new { id = product.Id },
			product);
	}

	// PUT: api/products/1
	[HttpPut("{id:int}")]
	public async Task<IActionResult> UpdateProduct(
		int id,
		Product updatedProduct)
	{
		var product = await _context.Products
			.FirstOrDefaultAsync(p => p.Id == id);

		if (product == null)
		{
			return NotFound(new
			{
				message = "Product not found."
			});
		}

		if (string.IsNullOrWhiteSpace(updatedProduct.Name))
		{
			return BadRequest(new
			{
				message = "Product name is required."
			});
		}

		if (updatedProduct.Price <= 0)
		{
			return BadRequest(new
			{
				message = "Price must be greater than zero."
			});
		}

		if (updatedProduct.StockQuantity < 0)
		{
			return BadRequest(new
			{
				message = "Stock quantity cannot be negative."
			});
		}

		product.Name = updatedProduct.Name;
		product.Price = updatedProduct.Price;
		product.StockQuantity = updatedProduct.StockQuantity;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return Conflict(new
			{
				message = "Product was modified by another request."
			});
		}

		return Ok(product);
	}

	// DELETE: api/products/1
	[HttpDelete("{id:int}")]
	public async Task<IActionResult> DeleteProduct(int id)
	{
		var product = await _context.Products
			.FirstOrDefaultAsync(p => p.Id == id);

		if (product == null)
		{
			return NotFound(new
			{
				message = "Product not found."
			});
		}

		_context.Products.Remove(product);

		await _context.SaveChangesAsync();

		return NoContent();
	}
}