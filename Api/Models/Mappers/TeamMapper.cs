using Api.Models.Dtos;

namespace Api.Models.Mappers;

public static class TeamMapper
{
	public static TeamResponse ToResponse(this Team team)
	{
		return new TeamResponse(
			team.Key,
			team.Name,
			team.Coaches.Select(c => c.ToResponse()).ToList(),
			team.Players.Select(p => p.ToResponse()).ToList()
		);
	}

	public static CoachResponse ToResponse(this Coach coach)
	{
		return new CoachResponse(
			coach.Key,
			coach.Name,
			coach.Type.ToString()
		);
	}

	public static PlayerResponse ToResponse(this Player player)
	{
		return new PlayerResponse(
			player.Key,
			player.LastName,
			player.Number
		);
	}

	public static Team ToEntity(this CreateTeamRequest request, string userId)
	{
		return new Team
		{
			Name = request.Name,
			UserId = userId,
			Coaches = request.Coaches.Select(c => new Coach
			{
				Name = c.Name,
				Type = Enum.Parse<CoachType>(c.Type, ignoreCase: true),
			}).ToList(),
			Players = request.Players.Select(p => new Player
			{
				LastName = p.LastName,
				Number = p.Number,
			}).ToList(),
		};
	}
}
