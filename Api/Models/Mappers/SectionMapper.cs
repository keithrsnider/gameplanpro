using Api.Models.Dtos;

namespace Api.Models.Mappers;

public static class SectionMapper
{
	public static SectionResponse ToResponse(this Section section)
	{
		return new SectionResponse(
			section.Key,
			section.Name,
			section.DisplayOrder,
			section.PlanDrills.Select(pd => pd.ToResponse()).ToList()
		);
	}
}
