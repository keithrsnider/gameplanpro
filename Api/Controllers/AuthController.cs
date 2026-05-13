using Api.Extensions;
using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimitingServiceExtensions.AuthPolicy)]
public class AuthController(IAuthService authService) : ControllerBase
{
	[HttpPost("register")]
	public Task<AuthUserResponse> Register([FromBody] RegisterRequest request)
	{
		return authService.RegisterAsync(request);
	}

	[HttpPost("login")]
	public Task<AuthUserResponse> Login([FromBody] LoginRequest request)
	{
		return authService.LoginAsync(request);
	}

	[Authorize]
	[HttpPost("reset-password")]
	public Task ResetPassword([FromBody] ResetPasswordRequest request)
	{
		return authService.ResetPasswordAsync(User, request);
	}

	[Authorize]
	[HttpPost("logout")]
	public Task Logout()
	{
		return authService.LogoutAsync();
	}

	[Authorize]
	[HttpGet("me")]
	public Task<AuthUserResponse> Me()
	{
		return authService.GetCurrentUserAsync(User);
	}
}
