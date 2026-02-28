namespace Api.Models.Dtos;

public record PlanDrillResponse(
	Guid Key,
	string Name,
	int Duration,
	string? Instructions,
	string? DemoLink,
	string? CoachAssignment,
	int? PlayerCount,
	Guid? StationGroup,
	int DisplayOrder,
	DrillTypeResponse? DrillType,
	Guid? SourceDrillKey
);

public record CreatePlanDrillRequest(
	string Name,
	int Duration,
	string? Instructions,
	string? DemoLink,
	string? CoachAssignment,
	int? PlayerCount,
	Guid? StationGroup,
	int DisplayOrder,
	int? DrillTypeId,
	Guid? SourceDrillKey
);

public record UpdatePlanDrillRequest(
	string? Name,
	int? Duration,
	string? Instructions,
	string? DemoLink,
	string? CoachAssignment,
	int? PlayerCount,
	Guid? StationGroup,
	int? DisplayOrder,
	int? DrillTypeId
);
