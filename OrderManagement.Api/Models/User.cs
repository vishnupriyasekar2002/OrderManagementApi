using OrderManagementApi.Models;

namespace OrderManagement.Api.Models;

public class User
{
	public int Id { get; set; }

	public string Username { get; set; } = string.Empty;

	public string Password { get; set; } = string.Empty;

	public string Role { get; set; } = "Customer";

	public ICollection<Order> Orders { get; set; } = new List<Order>();
}