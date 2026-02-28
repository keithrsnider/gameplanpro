using Api.Models.Dtos;

namespace Api.Models.Mappers;

public static class PracticePlanMapper
{
	public static PracticePlanListResponse ToListResponse(this PracticePlan plan)
	{
		return new PracticePlanListResponse(
			plan.Key, plan.Name, plan.Location, plan.IntendedDuration,
			plan.CreatedAt, plan.LastModifiedAt
		);
	}

	public static PracticePlanDetailResponse ToDetailResponse(this PracticePlan plan)
	{
		return new PracticePlanDetailResponse(
			plan.Key,
			plan.Name,
			plan.Location,
			plan.IntendedDuration,
			plan.Description,
			plan.CreatedAt,
			plan.LastModifiedAt,
			plan.Sections.Select(s => s.ToResponse()).ToList()
		);
	}

	public static PracticePlan ToEntity(this CreatePracticePlanRequest request, string userId)
	{
		return new PracticePlan
		{
			Name = request.Name,
			Location = request.Location,
			IntendedDuration = request.IntendedDuration,
			Description = request.Description,
			UserId = userId,
			Sections = [new Section { Name = "Section 1", DisplayOrder = 0 }],
		};
	}
}
