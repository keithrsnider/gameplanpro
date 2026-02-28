using Api.Models.Dtos;

namespace Api.Models.Mappers;

public static class DrillTypeMapper
{
	public static DrillTypeResponse ToResponse(this DrillType drillType)
	{
		return new DrillTypeResponse(drillType.Key, drillType.Name);
	}
}
