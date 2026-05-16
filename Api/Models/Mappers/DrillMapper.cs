using Api.Models.Dtos;

namespace Api.Models.Mappers;

public static class DrillMapper
{
	public static DrillResponse ToResponse(this Drill drill)
	{
		return new DrillResponse(
			drill.Key,
			drill.Name,
			drill.Description,
			drill.Duration,
			drill.Instructions,
			drill.DemoLink,
			drill.NumberOfPlayers,
			drill.Source.ToString(),
			drill.Coach?.ToResponse(),
			drill.DrillType.ToResponse(),
			drill.CreatedAt,
			drill.UpdatedAt
		);
	}

	public static Drill ToEntity(this CreateDrillRequest request, string userId, int drillTypeId)
	{
		return new Drill
		{
			Name = request.Name,
			Description = request.Description,
			Duration = request.Duration,
			Instructions = request.Instructions,
			DemoLink = request.DemoLink,
			NumberOfPlayers = request.NumberOfPlayers,
			CoachId = request.CoachId,
			Source = DrillSource.User,
			DrillTypeId = drillTypeId,
			UserId = userId,
		};
	}
}
