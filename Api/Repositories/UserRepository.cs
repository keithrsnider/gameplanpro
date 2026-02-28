using Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Api.Repositories;

public interface IUserRepository
{
	Task<AppUser?> GetByIdAsync(string userId);
}

public class UserRepository(UserManager<AppUser> userManager) : IUserRepository
{
	public Task<AppUser?> GetByIdAsync(string userId)
	{
		return userManager.FindByIdAsync(userId);
	}
}
