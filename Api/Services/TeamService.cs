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
		ValidatePlayers(request.Players);

		var team = request.ToEntity(userContext.UserId);
		await teamRepo.CreateAsync(team);
		return team.ToResponse();
	}

	public async Task<TeamResponse> UpdateAsync(UpdateTeamRequest request)
	{
		var team = await teamRepo.GetByUserIdAsync(userContext.UserId)
			?? throw new BadHttpRequestException("Team not found.", StatusCodes.Status404NotFound);

		ValidateCoaches(request.Coaches);
		ValidatePlayers(request.Players);

		team.Name = request.Name;

		// Build lookup of existing coaches by key
		var existingCoachesByKey = team.Coaches.ToDictionary(c => c.Key);
		var incomingCoachKeys = new HashSet<Guid>();

		foreach (var c in request.Coaches)
		{
			var type = Enum.Parse<CoachType>(c.Type, ignoreCase: true);

			if (c.Key is { } key && existingCoachesByKey.TryGetValue(key, out var existing))
			{
				existing.Name = c.Name;
				existing.Type = type;
				incomingCoachKeys.Add(key);
			}
			else
			{
				team.Coaches.Add(new Coach { Name = c.Name, Type = type });
			}
		}

		var coachesToRemove = team.Coaches.Where(c => existingCoachesByKey.ContainsKey(c.Key) && !incomingCoachKeys.Contains(c.Key)).ToList();
		foreach (var c in coachesToRemove)
			team.Coaches.Remove(c);

		// Build lookup of existing players by key
		var existingPlayersByKey = team.Players.ToDictionary(p => p.Key);
		var incomingPlayerKeys = new HashSet<Guid>();

		foreach (var p in request.Players)
		{
			if (p.Key is { } key && existingPlayersByKey.TryGetValue(key, out var existing))
			{
				existing.LastName = p.LastName;
				existing.Number = p.Number;
				incomingPlayerKeys.Add(key);
			}
			else
			{
				team.Players.Add(new Player { LastName = p.LastName, Number = p.Number });
			}
		}

		var playersToRemove = team.Players.Where(p => existingPlayersByKey.ContainsKey(p.Key) && !incomingPlayerKeys.Contains(p.Key)).ToList();
		foreach (var p in playersToRemove)
			team.Players.Remove(p);

		await teamRepo.UpdateAsync(team);
		return team.ToResponse();
	}

	private static void ValidatePlayers(List<PlayerRequest> players)
	{
		foreach (var p in players)
		{
			if (string.IsNullOrWhiteSpace(p.LastName))
				throw new BadHttpRequestException(
					"Player last name is required.", StatusCodes.Status400BadRequest
				);
		}
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
