namespace Api.Models.Dtos;

public record DrillResponse(
	Guid Key,
	string Name,
	string? Description,
	int Duration,
	string? Instructions,
	string? DemoLink,
	int? PlayerCount,
	string Source,
	CoachResponse? Coach,
	DrillTypeResponse DrillType,
	DateTime CreatedAt,
	DateTime UpdatedAt
);

public record CreateDrillRequest(
	string Name,
	string? Description,
	int Duration,
	string? Instructions,
	string? DemoLink,
	int? PlayerCount,
	int? CoachId,
	int DrillTypeId
);

public record UpdateDrillRequest(
	string Name,
	string? Description,
	int Duration,
	string? Instructions,
	string? DemoLink,
	int? PlayerCount,
	int? CoachId,
	int DrillTypeId
);
