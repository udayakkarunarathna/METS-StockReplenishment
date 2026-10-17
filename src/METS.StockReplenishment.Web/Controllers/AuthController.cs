using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace METS.StockReplenishment.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly UserManager<IdentityUser> _userManager;

	public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
	{
		_signInManager = signInManager;
		_userManager = userManager;
	}

	[HttpGet("test")]
	public IActionResult Test()
	{
		return Ok(new { message = "AuthController is working!" });
	}

	[HttpPost("login")]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
		{
			return BadRequest(new { message = "Username and password are required" });
		}

		var user = await _userManager.FindByNameAsync(request.Username);
		if (user == null)
		{
			return Unauthorized(new { message = "Invalid username or password" });
		}

		var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
		if (!result.Succeeded)
		{
			return Unauthorized(new { message = "Invalid username or password" });
		}

		return Ok(new { message = "Login successful", username = user.UserName });
	}

	[HttpPost("logout")]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
		return Ok(new { message = "Logout successful" });
	}

	[HttpGet("current-user")]
	public async Task<IActionResult> GetCurrentUser()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return Ok(new { isAuthenticated = false });
		}

		var roles = await _userManager.GetRolesAsync(user);
		return Ok(new
		{
			isAuthenticated = true,
			username = user.UserName,
			email = user.Email,
			roles = roles
		});
	}
}

public class LoginRequest
{
	public string Username { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;

}

