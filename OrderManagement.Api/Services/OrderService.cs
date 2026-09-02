using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;
using OrderManagementApi.Models;

namespace OrderManagement.Api.Services;

public class OrderService
{
	private readonly AppDbContext _context;

	public OrderService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<(bool Success, string Message, Order? Order)> CreateOrder(
		int userId,
		string idempotencyKey,
		List<OrderItem> items)
	{
		if (string.IsNullOrWhiteSpace(idempotencyKey))
		{
			return (false, "Idempotency-Key is required.", null);
		}

		if (items == null || items.Count == 0)
		{
			return (false, "Order must contain at least one product.", null);
		}

		// Check if the same request was already processed
		var existingOrder = await _context.Orders
			.Include(o => o.OrderItems)
			.FirstOrDefaultAsync(o =>
				o.UserId == userId &&
				o.IdempotencyKey == idempotencyKey);

		if (existingOrder != null)
		{
			return (true, "Order already exists.", existingOrder);
		}

		await using var transaction =
			await _context.Database.BeginTransactionAsync();

			decimal total = 0;

			var order = new Order
			{
				UserId = userId,
				OrderDate = DateTime.UtcNow,
				Status = "Confirmed",
				IdempotencyKey = idempotencyKey
			};

			foreach (var item in items)
			{
				if (item.Quantity <= 0)
				{
					await transaction.RollbackAsync();

					return (false, "Quantity must be greater than zero.", null);
				}

			var product = await _context.Products
.FirstOrDefaultAsync(p => p.Id == item.ProductId);

			if (product == null)
			{
				await transaction.RollbackAsync();

				return (false,
					$"Product {item.ProductId} not found.",
					null);
			}

			var rowsUpdated = await _context.Products
				.Where(p => p.Id == item.ProductId &&
							p.StockQuantity >= item.Quantity)
				.ExecuteUpdateAsync(setters =>
					setters.SetProperty(
						p => p.StockQuantity,
						p => p.StockQuantity - item.Quantity));

			if (rowsUpdated == 0)
			{
				await transaction.RollbackAsync();

				return (false,
					$"Insufficient stock for {product.Name}.",
					null);
			}

			var orderItem = new OrderItem
				{
					ProductId = product.Id,
					Quantity = item.Quantity,
					UnitPrice = product.Price
				};

				order.OrderItems.Add(orderItem);

				total += product.Price * item.Quantity;
			}

			order.TotalAmount = total;

			_context.Orders.Add(order);

			await _context.SaveChangesAsync();

			await transaction.CommitAsync();

			return (true, "Order created successfully.", order);
	}
}