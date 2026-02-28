using Api.Models.Dtos;

namespace Api.Models.Mappers;

public static class AppUserMapper
{
	public static AuthUserResponse ToResponse(this AppUser user)
	{
		return new AuthUserResponse(user.Id, user.Email!, user.DisplayName);
	}
}
