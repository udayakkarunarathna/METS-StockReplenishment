using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace METS.StockReplenishment.Web.Controllers;

[Route("Account")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AccountController : Controller
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly SignInManager<IdentityUser> _signInManager;

	public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
	{
		_userManager = userManager;
		_signInManager = signInManager;
	}

	[HttpPost("login")]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Login([FromForm] LoginViewModel model, string? returnUrl = null)
	{
		if (!ModelState.IsValid)
		{
			return RedirectToAction("LoginPage", new { error = "Invalid credentials" });
		}

		var user = await _userManager.FindByNameAsync(model.Username);
		if (user == null)
		{
			return RedirectToAction("LoginPage", new { error = "Invalid username or password" });
		}

		var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
		if (!result.Succeeded)
		{
			return RedirectToAction("LoginPage", new { error = "Invalid username or password" });
		}

		await _signInManager.SignInAsync(user, isPersistent: false);

		return LocalRedirect(returnUrl ?? "/requests");
	}

	[HttpPost("logout")]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
		return RedirectToAction("LoginPage");
	}

	[HttpGet("login")]
	public IActionResult LoginPage(string? error = null)
	{
		if (string.IsNullOrWhiteSpace(error))
		{
			return Redirect("/login");
		}

		return Redirect($"/login?error={Uri.EscapeDataString(error)}");
	}
}

public class LoginViewModel
{
	[Required(ErrorMessage = "Username is required")]
	public string Username { get; set; } = string.Empty;

	[Required(ErrorMessage = "Password is required")]
	[DataType(DataType.Password)]
	public string Password { get; set; } = string.Empty;

}
