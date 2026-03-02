namespace Api.Models.Dtos;

public record TeamResponse(
	Guid Key,
	string Name,
	List<CoachResponse> Coaches
);

public record CoachResponse(
	Guid Key,
	string Name,
	string Type
);

public record CoachRequest(
	Guid? Key,
	string Name,
	string Type
);

public record CreateTeamRequest(
	string Name,
	List<CoachRequest> Coaches
);

public record UpdateTeamRequest(
	string Name,
	List<CoachRequest> Coaches
);
