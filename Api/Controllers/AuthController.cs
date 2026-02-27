using Api.Extensions;
using Api.Models;
using Api.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimitingServiceExtensions.AuthPolicy)]
public class AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
	: ControllerBase
{
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		var user = new AppUser
		{
			Email = request.Email,
			UserName = request.Email,
			DisplayName = request.DisplayName,
		};

		var result = await userManager.CreateAsync(user, request.Password);

		if (!result.Succeeded)
			return BadRequest(result.Errors);

		await signInManager.SignInAsync(user, isPersistent: false);

		return Ok(new AuthUserResponse(user.Id, user.Email!, user.DisplayName));
	}
}
