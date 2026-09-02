using OrderManagement.Api.Models;

namespace OrderManagementApi.Models
{
	public class Order
	{
		public int Id { get; set; }

		public int UserId { get; set; }

		public DateTime OrderDate { get; set; }

		public string Status { get; set; } = "Pending";

		public decimal TotalAmount { get; set; }

		public string IdempotencyKey { get; set; } = string.Empty;

		public User? User { get; set; }

		public ICollection<OrderItem> OrderItems { get; set; }
			= new List<OrderItem>();
	}
}

