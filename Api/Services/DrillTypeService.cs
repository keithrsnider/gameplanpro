using Api.Models.Dtos;
using Api.Models.Mappers;
using Api.Repositories;

namespace Api.Services;

public interface IDrillTypeService
{
	Task<List<DrillTypeResponse>> GetAllAsync();
}

public class DrillTypeService(IDrillTypeRepository drillTypeRepo) : IDrillTypeService
{
	public async Task<List<DrillTypeResponse>> GetAllAsync()
	{
		var types = await drillTypeRepo.GetAllAsync();
		return types.Select(dt => dt.ToResponse()).ToList();
	}
}
