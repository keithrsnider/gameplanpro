using Api.Models;
using Api.Models.Dtos;
using Api.Models.Mappers;
using Api.Repositories;

namespace Api.Services;

public interface IDrillService
{
	Task<List<DrillResponse>> GetAllAsync(string userId, DrillSource? source, Guid? drillTypeKey);
	Task<DrillResponse> GetByKeyAsync(string userId, Guid key);
	Task<DrillResponse> CreateAsync(string userId, CreateDrillRequest request);
	Task<DrillResponse> UpdateAsync(string userId, Guid key, UpdateDrillRequest request);
	Task DeleteAsync(string userId, Guid key);
}

public class DrillService(IDrillRepository drillRepo, IDrillTypeRepository drillTypeRepo)
	: IDrillService
{
	public async Task<List<DrillResponse>> GetAllAsync(
		string userId, DrillSource? source, Guid? drillTypeKey)
	{
		int? drillTypeId = null;
		if (drillTypeKey is not null)
		{
			var drillType = await drillTypeRepo.GetByKeyAsync(drillTypeKey.Value);
			if (drillType is null)
				throw new BadHttpRequestException("Drill type not found.", StatusCodes.Status404NotFound);
			drillTypeId = drillType.Id;
		}

		var drills = await drillRepo.GetAllAsync(userId, source, drillTypeId);
		return drills.Select(d => d.ToResponse()).ToList();
	}

	public async Task<DrillResponse> GetByKeyAsync(string userId, Guid key)
	{
		var drill = await drillRepo.GetByKeyAsync(key)
			?? throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		if (drill.Source == DrillSource.User && drill.UserId != userId)
			throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		return drill.ToResponse();
	}

	public async Task<DrillResponse> CreateAsync(string userId, CreateDrillRequest request)
	{
		var drillType = await drillTypeRepo.GetByKeyAsync(request.DrillTypeKey)
			?? throw new BadHttpRequestException("Drill type not found.", StatusCodes.Status400BadRequest);

		var drill = request.ToEntity(userId, drillType.Id);

		await drillRepo.CreateAsync(drill);
		drill.DrillType = drillType;
		return drill.ToResponse();
	}

	public async Task<DrillResponse> UpdateAsync(string userId, Guid key, UpdateDrillRequest request)
	{
		var drill = await drillRepo.GetByKeyAsync(key)
			?? throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		if (drill.Source == DrillSource.System)
			throw new BadHttpRequestException(
				"System drills cannot be edited.", StatusCodes.Status403Forbidden
			);

		if (drill.UserId != userId)
			throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		var drillType = await drillTypeRepo.GetByKeyAsync(request.DrillTypeKey)
			?? throw new BadHttpRequestException("Drill type not found.", StatusCodes.Status400BadRequest);

		drill.Name = request.Name;
		drill.Description = request.Description;
		drill.Duration = request.Duration;
		drill.Instructions = request.Instructions;
		drill.DemoLink = request.DemoLink;
		drill.DrillTypeId = drillType.Id;
		drill.DrillType = drillType;
		drill.UpdatedAt = DateTime.UtcNow;

		await drillRepo.UpdateAsync(drill);
		return drill.ToResponse();
	}

	public async Task DeleteAsync(string userId, Guid key)
	{
		var drill = await drillRepo.GetByKeyAsync(key)
			?? throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		if (drill.Source == DrillSource.System)
			throw new BadHttpRequestException(
				"System drills cannot be deleted.", StatusCodes.Status403Forbidden
			);

		if (drill.UserId != userId)
			throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		await drillRepo.DeleteAsync(drill);
	}
}
