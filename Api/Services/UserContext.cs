using System.Security.Claims;

namespace Api.Services;

public interface IUserContext
{
	string UserId { get; }
}

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
	public string UserId =>
		httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
		?? throw new UnauthorizedAccessException("User is not authenticated.");
}
