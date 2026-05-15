namespace Api.Models.Dtos;

public record TeamResponse(
	Guid Key,
	string Name,
	List<CoachResponse> Coaches,
	List<PlayerResponse> Players
);

public record CoachResponse(
	int Id,
	Guid Key,
	string Name,
	string Type
);

public record CoachRequest(
	Guid? Key,
	string Name,
	string Type
);

public record PlayerResponse(
	Guid Key,
	string LastName,
	int Number
);

public record PlayerRequest(
	Guid? Key,
	string LastName,
	int Number
);

public record CreateTeamRequest(
	string Name,
	List<CoachRequest> Coaches,
	List<PlayerRequest> Players
);

public record UpdateTeamRequest(
	string Name,
	List<CoachRequest> Coaches,
	List<PlayerRequest> Players
);
