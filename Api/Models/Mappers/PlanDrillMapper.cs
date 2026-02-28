using Api.Models.Dtos;

namespace Api.Models.Mappers;

public static class PlanDrillMapper
{
	public static PlanDrillResponse ToResponse(this PlanDrill pd)
	{
		return new PlanDrillResponse(
			pd.Key,
			pd.Name,
			pd.Duration,
			pd.Instructions,
			pd.DemoLink,
			pd.CoachAssignment,
			pd.PlayerCount,
			pd.StationGroup,
			pd.DisplayOrder,
			pd.DrillType?.ToResponse(),
			pd.Drill?.Key
		);
	}

	public static PlanDrill ToEntity(
		this CreatePlanDrillRequest request, int sectionId, int? drillTypeId, int? drillId)
	{
		return new PlanDrill
		{
			Name = request.Name,
			Duration = request.Duration,
			Instructions = request.Instructions,
			DemoLink = request.DemoLink,
			CoachAssignment = request.CoachAssignment,
			PlayerCount = request.PlayerCount,
			StationGroup = request.StationGroup,
			DisplayOrder = request.DisplayOrder,
			SectionId = sectionId,
			DrillTypeId = drillTypeId,
			DrillId = drillId,
		};
	}
}
