namespace Api.Models.Dtos;

public record DrillResponse(
	Guid Key,
	string Name,
	string? Description,
	int Duration,
	string? Instructions,
	string? DemoLink,
	string Source,
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
	int DrillTypeId
);

public record UpdateDrillRequest(
	string Name,
	string? Description,
	int Duration,
	string? Instructions,
	string? DemoLink,
	int DrillTypeId
);
