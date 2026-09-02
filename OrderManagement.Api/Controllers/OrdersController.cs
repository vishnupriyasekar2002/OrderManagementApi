using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Models;
using OrderManagement.Api.Services;
using System.Security.Claims;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
	private readonly OrderService _orderService;

	public OrdersController(OrderService orderService)
	{
		_orderService = orderService;
	}

	[HttpPost]
	public async Task<IActionResult> CreateOrder(
		[FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
		List<OrderItem> items)
	{
		var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

		if (userIdClaim == null)
		{
			return Unauthorized();
		}

		var userId = int.Parse(userIdClaim.Value);

		var result = await _orderService.CreateOrder(
			userId,
			idempotencyKey,
			items);

		if (!result.Success)
		{
			return BadRequest(new
			{
				message = result.Message
			});
		}

		return Ok(new
		{
			message = result.Message,
			orderId = result.Order!.Id,
			userId = result.Order.UserId,
			totalAmount = result.Order.TotalAmount,
			status = result.Order.Status
		});
	}
}