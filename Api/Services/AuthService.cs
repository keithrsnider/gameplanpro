using Api.Exceptions;
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
	Task ResetPasswordAsync(ClaimsPrincipal principal, ResetPasswordRequest request);
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
			throw new ValidationException(result.Errors.First().Description);

		await signInManager.SignInAsync(user, isPersistent: false);

		return user.ToResponse();
	}

	public async Task<AuthUserResponse> LoginAsync(LoginRequest request)
	{
		var user = await userManager.FindByEmailAsync(request.Email);

		if (user is null)
			throw new UnauthorizedAccessException("Invalid email or password.");

		var result = await signInManager.PasswordSignInAsync(
			user, request.Password, isPersistent: false, lockoutOnFailure: false
		);

		if (!result.Succeeded)
			throw new UnauthorizedAccessException("Invalid email or password.");

		return user.ToResponse();
	}

	public async Task ResetPasswordAsync(ClaimsPrincipal principal, ResetPasswordRequest request)
	{
		var user = await userManager.GetUserAsync(principal);

		if (user is null)
			throw new UnauthorizedAccessException("Unauthorized.");

		var result = await userManager.ChangePasswordAsync(
			user,
			request.CurrentPassword,
			request.NewPassword
		);

		if (!result.Succeeded)
			throw new ValidationException(result.Errors.First().Description);

		await signInManager.RefreshSignInAsync(user);
	}

	public Task LogoutAsync()
	{
		return signInManager.SignOutAsync();
	}

	public async Task<AuthUserResponse> GetCurrentUserAsync(ClaimsPrincipal principal)
	{
		var user = await userManager.GetUserAsync(principal);

		if (user is null)
			throw new UnauthorizedAccessException("Unauthorized.");

		return user.ToResponse();
	}
}
