using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Api.Data;
using OrderManagement.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _configuration;

	public AuthController(
		AppDbContext context,
		IConfiguration configuration)
	{
		_context = context;
		_configuration = configuration;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register(User user)
	{
		if (string.IsNullOrWhiteSpace(user.Username) ||
			string.IsNullOrWhiteSpace(user.Password))
		{
			return BadRequest(new
			{
				message = "Username and password are required."
			});
		}

		var exists = await _context.Users
			.AnyAsync(u => u.Username == user.Username);

		if (exists)
		{
			return Conflict(new
			{
				message = "Username already exists."
			});
		}

		user.Role = "Customer";

		_context.Users.Add(user);

		await _context.SaveChangesAsync();

		return Ok(new
		{
			message = "User registered successfully."
		});
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login(User login)
	{
		var user = await _context.Users
			.FirstOrDefaultAsync(u =>
				u.Username == login.Username &&
				u.Password == login.Password);

		if (user == null)
		{
			return Unauthorized(new
			{
				message = "Invalid username or password."
			});
		}

		var token = GenerateToken(user);

		return Ok(new
		{
			token
		});
	}

	private string GenerateToken(User user)
	{
		var claims = new[]
		{
			new Claim(
				ClaimTypes.NameIdentifier,
				user.Id.ToString()),

			new Claim(
				ClaimTypes.Name,
				user.Username),

			new Claim(
				ClaimTypes.Role,
				user.Role)
		};

		var key = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(
				_configuration["Jwt:Key"]!));

		var credentials = new SigningCredentials(
			key,
			SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _configuration["Jwt:Issuer"],
			audience: _configuration["Jwt:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddHours(1),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler()
			.WriteToken(token);
	}
}