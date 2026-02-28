using Api.Models;
using Api.Models.Dtos;
using Api.Models.Mappers;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Api.Services;

public interface IAuthService
{
	Task<AuthUserResponse> RegisterAsync(RegisterRequest request);
	Task<AuthUserResponse> LoginAsync(LoginRequest request);
	Task LogoutAsync();
	Task<AuthUserResponse> GetCurrentUserAsync(ClaimsPrincipal principal);
}

public class AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
	: IAuthService
{
	public async Task<AuthUserResponse> RegisterAsync(RegisterRequest request)
	{
		var user = new AppUser
		{
			Email = request.Email,
			UserName = request.Email,
			DisplayName = request.DisplayName,
		};

		var result = await userManager.CreateAsync(user, request.Password);

		if (!result.Succeeded)
			throw new BadHttpRequestException(
				result.Errors.First().Description,
				StatusCodes.Status400BadRequest
			);

		await signInManager.SignInAsync(user, isPersistent: false);

		return user.ToResponse();
	}

	public async Task<AuthUserResponse> LoginAsync(LoginRequest request)
	{
		var user = await userManager.FindByEmailAsync(request.Email);

		if (user is null)
			throw new BadHttpRequestException(
				"Invalid email or password.",
				StatusCodes.Status401Unauthorized
			);

		var result = await signInManager.PasswordSignInAsync(
			user, request.Password, isPersistent: false, lockoutOnFailure: false
		);

		if (!result.Succeeded)
			throw new BadHttpRequestException(
				"Invalid email or password.",
				StatusCodes.Status401Unauthorized
			);

		return user.ToResponse();
	}

	public Task LogoutAsync()
	{
		return signInManager.SignOutAsync();
	}

	public async Task<AuthUserResponse> GetCurrentUserAsync(ClaimsPrincipal principal)
	{
		var user = await userManager.GetUserAsync(principal);

		if (user is null)
			throw new BadHttpRequestException("Unauthorized.", StatusCodes.Status401Unauthorized);

		return user.ToResponse();
	}
}
