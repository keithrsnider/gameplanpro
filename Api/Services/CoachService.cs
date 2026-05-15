using Api.Models.Dtos;
using Api.Models.Mappers;
using Api.Repositories;

namespace Api.Services;

public interface ICoachService
{
	Task<List<CoachResponse>> GetByTeamAsync();
}

public class CoachService(
	ITeamRepository teamRepo,
	IUserContext userContext
) : ICoachService
{
	public async Task<List<CoachResponse>> GetByTeamAsync()
	{
		var team = await teamRepo.GetByUserIdAsync(userContext.UserId);
		if (team is null)
			return [];

		return team.Coaches
			.OrderBy(c => c.Type)
			.ThenBy(c => c.Name)
			.Select(c => c.ToResponse())
			.ToList();
	}
}

