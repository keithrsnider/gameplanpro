using Api.Exceptions;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Api.Services;

public interface IAuthService
{
	Task<AuthUserResponse> RegisterAsync(RegisterRequest request);
	Task<AuthUserResponse> LoginAsync(LoginRequest request);
	Task ForgotPasswordAsync(ForgotPasswordRequest request);
	Task ResetPasswordAsync(ResetPasswordRequest request);
	Task ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequest request);
	Task LogoutAsync();
	Task<AuthUserResponse> GetCurrentUserAsync(ClaimsPrincipal principal);
}

public class AuthService(
	UserManager<AppUser> userManager,
	SignInManager<AppUser> signInManager,
	IEmailSender emailSender,
	IConfiguration config)
	: IAuthService
{
	private const string InvalidResetLinkMessage = "The reset link is invalid or has expired.";

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

	public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
	{
		var user = await userManager.FindByEmailAsync(request.Email);

		if (user is null || string.IsNullOrWhiteSpace(user.Email))
			return;

		var token = await userManager.GeneratePasswordResetTokenAsync(user);
		var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
		var resetUrl = QueryHelpers.AddQueryString(
			$"{GetClientBaseUrl()}/reset-password",
			new Dictionary<string, string?>
			{
				["email"] = user.Email,
				["token"] = encodedToken,
			}
		);

		var safeResetUrl = HtmlEncoder.Default.Encode(resetUrl);

		await emailSender.SendEmailAsync(
			user.Email,
			"Reset your GamePlanPro password",
			$$"""
				<p>We received a request to reset your GamePlanPro password.</p>
				<p>
					<a href="{{safeResetUrl}}">Reset your password</a>
				</p>
				<p>If you didn't request this, you can safely ignore this email.</p>
				<p>This link will expire automatically.</p>
				"""
		);
	}

	public async Task ResetPasswordAsync(ResetPasswordRequest request)
	{
		var user = await userManager.FindByEmailAsync(request.Email);

		if (user is null)
			throw new ValidationException(InvalidResetLinkMessage);

		string token;

		try
		{
			token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
		}
		catch (FormatException)
		{
			throw new ValidationException(InvalidResetLinkMessage);
		}

		var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

		if (!result.Succeeded)
			throw new ValidationException(result.Errors.First().Description);
	}

	public async Task ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequest request)
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

	private string GetClientBaseUrl()
	{
		var clientBaseUrl = config["App:ClientBaseUrl"];

		if (string.IsNullOrWhiteSpace(clientBaseUrl))
			throw new InvalidOperationException("App:ClientBaseUrl configuration is missing.");

		return clientBaseUrl.TrimEnd('/');
	}
}
