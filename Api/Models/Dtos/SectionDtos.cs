namespace Api.Models.Dtos;

public record SectionResponse(
	Guid Key,
	string Name,
	int DisplayOrder,
	string? Note,
	List<PlanDrillResponse> PlanDrills
);

public record CreateSectionRequest(string Name, int DisplayOrder, string? Note = null);

public record UpdateSectionRequest(string? Name, int? DisplayOrder, string? Note = null);
