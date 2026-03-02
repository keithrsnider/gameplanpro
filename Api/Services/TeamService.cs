using Api.Models;
using Api.Models.Dtos;
using Api.Models.Mappers;
using Api.Repositories;

namespace Api.Services;

public interface ITeamService
{
	Task<TeamResponse?> GetAsync();
	Task<TeamResponse> CreateAsync(CreateTeamRequest request);
	Task<TeamResponse> UpdateAsync(UpdateTeamRequest request);
}

public class TeamService(
	ITeamRepository teamRepo,
	IUserContext userContext
) : ITeamService
{
	public async Task<TeamResponse?> GetAsync()
	{
		var team = await teamRepo.GetByUserIdAsync(userContext.UserId);
		return team?.ToResponse();
	}

	public async Task<TeamResponse> CreateAsync(CreateTeamRequest request)
	{
		var existing = await teamRepo.GetByUserIdAsync(userContext.UserId);
		if (existing is not null)
			throw new BadHttpRequestException(
				"Team already exists. Use PUT to update.", StatusCodes.Status409Conflict
			);

		ValidateCoaches(request.Coaches);

		var team = request.ToEntity(userContext.UserId);
		await teamRepo.CreateAsync(team);
		return team.ToResponse();
	}

	public async Task<TeamResponse> UpdateAsync(UpdateTeamRequest request)
	{
		var team = await teamRepo.GetByUserIdAsync(userContext.UserId)
			?? throw new BadHttpRequestException("Team not found.", StatusCodes.Status404NotFound);

		ValidateCoaches(request.Coaches);

		team.Name = request.Name;

		// Build lookup of existing coaches by key
		var existingByKey = team.Coaches.ToDictionary(c => c.Key);
		var incomingKeys = new HashSet<Guid>();

		foreach (var c in request.Coaches)
		{
			var type = Enum.Parse<CoachType>(c.Type, ignoreCase: true);

			if (c.Key is { } key && existingByKey.TryGetValue(key, out var existing))
			{
				// Update existing coach
				existing.Name = c.Name;
				existing.Type = type;
				incomingKeys.Add(key);
			}
			else
			{
				// Add new coach
				team.Coaches.Add(new Coach { Name = c.Name, Type = type });
			}
		}

		// Remove coaches not in the incoming list
		var toRemove = team.Coaches.Where(c => existingByKey.ContainsKey(c.Key) && !incomingKeys.Contains(c.Key)).ToList();
		foreach (var c in toRemove)
			team.Coaches.Remove(c);

		await teamRepo.UpdateAsync(team);
		return team.ToResponse();
	}

	private static void ValidateCoaches(List<CoachRequest> coaches)
	{
		var headCoaches = coaches.Count(c =>
			string.Equals(c.Type, nameof(CoachType.Head), StringComparison.OrdinalIgnoreCase));

		if (headCoaches != 1)
			throw new BadHttpRequestException(
				"Exactly one Head Coach is required.", StatusCodes.Status400BadRequest
			);

		foreach (var c in coaches)
		{
			if (string.IsNullOrWhiteSpace(c.Name))
				throw new BadHttpRequestException(
					"Coach name is required.", StatusCodes.Status400BadRequest
				);

			if (!Enum.TryParse<CoachType>(c.Type, ignoreCase: true, out _))
				throw new BadHttpRequestException(
					$"Invalid coach type: {c.Type}.", StatusCodes.Status400BadRequest
				);
		}
	}
}
