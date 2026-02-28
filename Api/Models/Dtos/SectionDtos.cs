namespace Api.Models.Dtos;

public record SectionResponse(
	Guid Key,
	string Name,
	int DisplayOrder,
	List<PlanDrillResponse> PlanDrills
);

public record CreateSectionRequest(string Name, int DisplayOrder);

public record UpdateSectionRequest(string? Name, int? DisplayOrder);
