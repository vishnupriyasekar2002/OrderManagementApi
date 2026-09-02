using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;
using OrderManagement.Api.Services;

namespace OrderManagement.Api.Tests;

public class OrderServiceTests
{
	public static async Task RunTests()
	{
		Console.WriteLine("Running OrderService tests...");

		await TestSuccessfulOrder();
		await TestInsufficientStock();
		await TestDuplicateOrder();

		Console.WriteLine("All OrderService tests passed.");
	}

	private static AppDbContext CreateDb()
	{
		var connection = new Microsoft.Data.Sqlite.SqliteConnection(
			"Data Source=:memory:");

		connection.Open();

		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite(connection)
			.Options;

		var db = new AppDbContext(options);

		db.Database.EnsureCreated();

		return db;
	}

	private static async Task AddTestUser(AppDbContext db)
	{
		db.Users.Add(new User
		{
			Id = 1,
			Username = "testuser",
			Password = "test123",
			Role = "Customer"
		});

		await db.SaveChangesAsync();
	}

	private static async Task TestSuccessfulOrder()
	{
		using var db = CreateDb();

		await AddTestUser(db);

		db.Products.Add(new Product
		{
			Id = 1,
			Name = "Laptop",
			Price = 50000,
			StockQuantity = 5
		});

		await db.SaveChangesAsync();

		var service = new OrderService(db);

		var items = new List<OrderItem>
		{
			new OrderItem
			{
				ProductId = 1,
				Quantity = 2
			}
		};

		var result = await service.CreateOrder(1,"TEST-001",items);

		if (!result.Success)
		{
			throw new Exception(
				$"TestSuccessfulOrder failed: {result.Message}");
		}

		db.ChangeTracker.Clear();

		var product = await db.Products
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == 1);

		if (product == null || product.StockQuantity != 3)
		{
			throw new Exception(
				$"Stock was not reduced correctly. Actual stock: {product?.StockQuantity}");
		}

		Console.WriteLine("PASS: Successful order");
	}

	private static async Task TestInsufficientStock()
	{
		using var db = CreateDb();

		await AddTestUser(db);

		db.Products.Add(new Product
		{
			Id = 1,
			Name = "Laptop",
			Price = 50000,
			StockQuantity = 2
		});

		await db.SaveChangesAsync();

		var service = new OrderService(db);

		var items = new List<OrderItem>
		{
			new OrderItem
			{
				ProductId = 1,
				Quantity = 5
			}
		};

		var result = await service.CreateOrder(
			1,
			"TEST-002",
			items);

		if (result.Success)
			throw new Exception(
				"Order should have failed because of insufficient stock.");

		var product = await db.Products.FindAsync(1);

		if (product == null || product.StockQuantity != 2)
			throw new Exception(
				"Stock changed even though the order failed.");

		Console.WriteLine("PASS: Insufficient stock");
	}

	private static async Task TestDuplicateOrder()
	{
		using var db = CreateDb();

		await AddTestUser(db);

		db.Products.Add(new Product
		{
			Id = 1,
			Name = "Mouse",
			Price = 1000,
			StockQuantity = 10
		});

		await db.SaveChangesAsync();

		var service = new OrderService(db);

		var items = new List<OrderItem>
		{
			new OrderItem
			{
				ProductId = 1,
				Quantity = 2
			}
		};

		var first = await service.CreateOrder(
			1,
			"TEST-003",
			items);

		var second = await service.CreateOrder(
			1,
			"TEST-003",
			items);

		if (!first.Success)
			throw new Exception("First order failed.");

		if (!second.Success)
			throw new Exception(
				"Duplicate order test failed.");

		if (second.Message != "Order already exists.")
			throw new Exception(
				"Duplicate order was not detected.");

		db.ChangeTracker.Clear();

		var product = await db.Products
			.AsNoTracking()
			.FirstOrDefaultAsync(p => p.Id == 1);

		if (product == null || product.StockQuantity != 8)
		{
			throw new Exception(
				$"Stock was reduced twice. Actual stock: {product?.StockQuantity}");
		}
	}
}