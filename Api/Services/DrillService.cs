using Api.Models;
using Api.Models.Dtos;
using Api.Models.Mappers;
using Api.Repositories;

namespace Api.Services;

public interface IDrillService
{
	Task<List<DrillResponse>> GetAllAsync(DrillSource? source, int? drillTypeId);
	Task<DrillResponse> GetByKeyAsync(Guid key);
	Task<DrillResponse> CreateAsync(CreateDrillRequest request);
	Task<DrillResponse> UpdateAsync(Guid key, UpdateDrillRequest request);
	Task DeleteAsync(Guid key);
}

public class DrillService(
	IDrillRepository drillRepo,
	IDrillTypeRepository drillTypeRepo,
	ITeamRepository teamRepo,
	IUserContext userContext
) : IDrillService
{
	public async Task<List<DrillResponse>> GetAllAsync(
		DrillSource? source, int? drillTypeId)
	{
		if (drillTypeId is not null)
		{
			var drillType = await drillTypeRepo.GetByIdAsync(drillTypeId.Value);
			if (drillType is null)
				throw new BadHttpRequestException("Drill type not found.", StatusCodes.Status404NotFound);
		}

		var drills = await drillRepo.GetAllAsync(userContext.UserId, source, drillTypeId);
		return drills.Select(d => d.ToResponse()).ToList();
	}

	public async Task<DrillResponse> GetByKeyAsync(Guid key)
	{
		var drill = await drillRepo.GetByKeyAsync(key)
			?? throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		if (drill.Source == DrillSource.User && drill.UserId != userContext.UserId)
			throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		return drill.ToResponse();
	}

	public async Task<DrillResponse> CreateAsync(CreateDrillRequest request)
	{
		var drillType = await drillTypeRepo.GetByIdAsync(request.DrillTypeId)
			?? throw new BadHttpRequestException("Drill type not found.", StatusCodes.Status400BadRequest);

		var coach = await GetValidatedCoachAsync(request.CoachId);

		var drill = request.ToEntity(userContext.UserId, drillType.Id);
		drill.Coach = coach;

		await drillRepo.CreateAsync(drill);
		drill.DrillType = drillType;
		return drill.ToResponse();
	}

	public async Task<DrillResponse> UpdateAsync(Guid key, UpdateDrillRequest request)
	{
		var drill = await drillRepo.GetByKeyAsync(key)
			?? throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		if (drill.Source == DrillSource.System)
			throw new BadHttpRequestException(
				"System drills cannot be edited.", StatusCodes.Status403Forbidden
			);

		if (drill.UserId != userContext.UserId)
			throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		var drillType = await drillTypeRepo.GetByIdAsync(request.DrillTypeId)
			?? throw new BadHttpRequestException("Drill type not found.", StatusCodes.Status400BadRequest);

		var coach = await GetValidatedCoachAsync(request.CoachId);

		drill.Name = request.Name;
		drill.Description = request.Description;
		drill.Duration = request.Duration;
		drill.Instructions = request.Instructions;
		drill.DemoLink = request.DemoLink;
		drill.NumberOfPlayers = request.NumberOfPlayers;
		drill.CoachId = coach?.Id;
		drill.Coach = coach;
		drill.DrillTypeId = drillType.Id;
		drill.DrillType = drillType;
		drill.UpdatedAt = DateTime.UtcNow;

		await drillRepo.UpdateAsync(drill);
		return drill.ToResponse();
	}

	public async Task DeleteAsync(Guid key)
	{
		var drill = await drillRepo.GetByKeyAsync(key)
			?? throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		if (drill.Source == DrillSource.System)
			throw new BadHttpRequestException(
				"System drills cannot be deleted.", StatusCodes.Status403Forbidden
			);

		if (drill.UserId != userContext.UserId)
			throw new BadHttpRequestException("Drill not found.", StatusCodes.Status404NotFound);

		await drillRepo.DeleteAsync(drill);
	}

	private async Task<Coach?> GetValidatedCoachAsync(int? coachId)
	{
		if (coachId is null)
			return null;

		var team = await teamRepo.GetByUserIdAsync(userContext.UserId)
			?? throw new BadHttpRequestException("Coach not found.", StatusCodes.Status400BadRequest);

		return team.Coaches.FirstOrDefault(c => c.Id == coachId.Value)
			?? throw new BadHttpRequestException("Coach not found.", StatusCodes.Status400BadRequest);
	}
}
