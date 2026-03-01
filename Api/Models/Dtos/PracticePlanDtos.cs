namespace Api.Models.Dtos;

public record PracticePlanListResponse(
	Guid Key,
	string Name,
	string? Location,
	int? IntendedDuration,
	DateOnly? PracticeDate,
	DateTime CreatedAt,
	DateTime LastModifiedAt
);

public record PracticePlanDetailResponse(
	Guid Key,
	string Name,
	string? Location,
	int? IntendedDuration,
	DateOnly? PracticeDate,
	string? Description,
	DateTime CreatedAt,
	DateTime LastModifiedAt,
	List<SectionResponse> Sections
);

public record CreatePracticePlanRequest(
	string Name,
	string? Location,
	int? IntendedDuration,
	string? Description,
	DateOnly? PracticeDate
);

public record UpdatePracticePlanRequest(
	string? Name,
	string? Location,
	int? IntendedDuration,
	string? Description,
	DateOnly? PracticeDate
);
