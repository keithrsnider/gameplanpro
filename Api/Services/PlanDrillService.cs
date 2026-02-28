using Api.Models.Dtos;
using Api.Models.Mappers;
using Api.Repositories;

namespace Api.Services;

public interface IPlanDrillService
{
	Task<PlanDrillResponse> CreateAsync(
		string userId, Guid planKey, Guid sectionKey, CreatePlanDrillRequest request
	);
	Task<PlanDrillResponse> UpdateAsync(
		string userId, Guid planKey, Guid sectionKey, Guid drillKey, UpdatePlanDrillRequest request
	);
	Task DeleteAsync(string userId, Guid planKey, Guid sectionKey, Guid drillKey);
}

public class PlanDrillService(
	IPlanDrillRepository planDrillRepo,
	ISectionRepository sectionRepo,
	IPracticePlanRepository planRepo,
	IDrillTypeRepository drillTypeRepo,
	IDrillRepository drillRepo
) : IPlanDrillService
{
	public async Task<PlanDrillResponse> CreateAsync(
		string userId, Guid planKey, Guid sectionKey, CreatePlanDrillRequest request)
	{
		var (plan, section) = await VerifyOwnershipChainAsync(userId, planKey, sectionKey);

		int? drillTypeId = null;
		Models.DrillType? drillType = null;
		if (request.DrillTypeId is not null)
		{
			drillType = await drillTypeRepo.GetByIdAsync(request.DrillTypeId.Value)
				?? throw new BadHttpRequestException(
					"Drill type not found.", StatusCodes.Status400BadRequest
				);
			drillTypeId = drillType.Id;
		}

		int? sourceDrillId = null;
		if (request.SourceDrillKey is not null)
		{
			var sourceDrill = await drillRepo.GetByKeyAsync(request.SourceDrillKey.Value)
				?? throw new BadHttpRequestException(
					"Source drill not found.", StatusCodes.Status400BadRequest
				);
			sourceDrillId = sourceDrill.Id;
		}

		var planDrill = request.ToEntity(section.Id, drillTypeId, sourceDrillId);

		await planDrillRepo.CreateAsync(planDrill);
		planDrill.DrillType = drillType;
		plan.LastModifiedAt = DateTime.UtcNow;
		await planRepo.UpdateAsync(plan);

		return planDrill.ToResponse();
	}

	public async Task<PlanDrillResponse> UpdateAsync(
		string userId, Guid planKey, Guid sectionKey, Guid drillKey,
		UpdatePlanDrillRequest request)
	{
		var (plan, _) = await VerifyOwnershipChainAsync(userId, planKey, sectionKey);

		var planDrill = await planDrillRepo.GetByKeyAsync(drillKey)
			?? throw new BadHttpRequestException(
				"Plan drill not found.", StatusCodes.Status404NotFound
			);

		if (planDrill.Section.PracticePlanId != plan.Id)
			throw new BadHttpRequestException(
				"Plan drill not found.", StatusCodes.Status404NotFound
			);

		if (request.Name is not null) planDrill.Name = request.Name;
		if (request.Duration is not null) planDrill.Duration = request.Duration.Value;
		if (request.Instructions is not null) planDrill.Instructions = request.Instructions;
		if (request.DemoLink is not null) planDrill.DemoLink = request.DemoLink;
		if (request.CoachAssignment is not null) planDrill.CoachAssignment = request.CoachAssignment;
		if (request.PlayerCount is not null) planDrill.PlayerCount = request.PlayerCount;
		if (request.StationGroup is not null) planDrill.StationGroup = request.StationGroup;
		if (request.DisplayOrder is not null) planDrill.DisplayOrder = request.DisplayOrder.Value;

		if (request.DrillTypeId is not null)
		{
			var drillType = await drillTypeRepo.GetByIdAsync(request.DrillTypeId.Value)
				?? throw new BadHttpRequestException(
					"Drill type not found.", StatusCodes.Status400BadRequest
				);
			planDrill.DrillTypeId = drillType.Id;
			planDrill.DrillType = drillType;
		}

		await planDrillRepo.UpdateAsync(planDrill);
		plan.LastModifiedAt = DateTime.UtcNow;
		await planRepo.UpdateAsync(plan);

		return planDrill.ToResponse();
	}

	public async Task DeleteAsync(
		string userId, Guid planKey, Guid sectionKey, Guid drillKey)
	{
		var (plan, _) = await VerifyOwnershipChainAsync(userId, planKey, sectionKey);

		var planDrill = await planDrillRepo.GetByKeyAsync(drillKey)
			?? throw new BadHttpRequestException(
				"Plan drill not found.", StatusCodes.Status404NotFound
			);

		if (planDrill.Section.PracticePlanId != plan.Id)
			throw new BadHttpRequestException(
				"Plan drill not found.", StatusCodes.Status404NotFound
			);

		await planDrillRepo.DeleteAsync(planDrill);
		plan.LastModifiedAt = DateTime.UtcNow;
		await planRepo.UpdateAsync(plan);
	}

	private async Task<(Models.PracticePlan plan, Models.Section section)> VerifyOwnershipChainAsync(
		string userId, Guid planKey, Guid sectionKey)
	{
		var plan = await planRepo.GetByKeyAsync(planKey)
			?? throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (plan.UserId != userId)
			throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		var section = await sectionRepo.GetByKeyAsync(sectionKey)
			?? throw new BadHttpRequestException(
				"Section not found.", StatusCodes.Status404NotFound
			);

		if (section.PracticePlanId != plan.Id)
			throw new BadHttpRequestException(
				"Section not found.", StatusCodes.Status404NotFound
			);

		return (plan, section);
	}
}
