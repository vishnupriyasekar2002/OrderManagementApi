using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Models;
using OrderManagementApi.Models;

namespace OrderManagement.Api.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
	{
	}

	public DbSet<User> Users => Set<User>();
	public DbSet<Product> Products => Set<Product>();
	public DbSet<Order> Orders => Set<Order>();
	public DbSet<OrderItem> OrderItems => Set<OrderItem>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Product>()
			.Property(p => p.Price)
			.HasPrecision(18, 2);

		modelBuilder.Entity<Order>()
			.Property(o => o.TotalAmount)
			.HasPrecision(18, 2);

		modelBuilder.Entity<OrderItem>()
			.Property(i => i.UnitPrice)
			.HasPrecision(18, 2);

		modelBuilder.Entity<Order>()
			.HasOne(o => o.User)
			.WithMany(u => u.Orders)
			.HasForeignKey(o => o.UserId);

		modelBuilder.Entity<OrderItem>()
			.HasOne(i => i.Order)
			.WithMany(o => o.OrderItems)
			.HasForeignKey(i => i.OrderId);

		modelBuilder.Entity<OrderItem>()
			.HasOne(i => i.Product)
			.WithMany(p => p.OrderItems)
			.HasForeignKey(i => i.ProductId);
	}
}