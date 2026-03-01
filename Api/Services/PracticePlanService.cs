using Api.Models.Dtos;
using Api.Models.Mappers;
using Api.Repositories;

namespace Api.Services;

public interface IPracticePlanService
{
	Task<List<PracticePlanListResponse>> GetAllAsync();
	Task<PracticePlanDetailResponse> GetByKeyAsync(Guid key);
	Task<PracticePlanDetailResponse> CreateAsync(CreatePracticePlanRequest request);
	Task<PracticePlanDetailResponse> UpdateAsync(Guid key, UpdatePracticePlanRequest request);
	Task DeleteAsync(Guid key);
}

public class PracticePlanService(
	IPracticePlanRepository planRepo,
	IUserContext userContext
) : IPracticePlanService
{
	public async Task<List<PracticePlanListResponse>> GetAllAsync()
	{
		var plans = await planRepo.GetAllByUserAsync(userContext.UserId);
		return plans.Select(pp => pp.ToListResponse()).ToList();
	}

	public async Task<PracticePlanDetailResponse> GetByKeyAsync(Guid key)
	{
		var plan = await planRepo.GetDetailByKeyAsync(key)
			?? throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (plan.UserId != userContext.UserId)
			throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		return plan.ToDetailResponse();
	}

	public async Task<PracticePlanDetailResponse> CreateAsync(CreatePracticePlanRequest request)
	{
		var plan = request.ToEntity(userContext.UserId);

		await planRepo.CreateAsync(plan);

		// Re-fetch with includes for proper response
		var created = await planRepo.GetDetailByKeyAsync(plan.Key);
		return created!.ToDetailResponse();
	}

	public async Task<PracticePlanDetailResponse> UpdateAsync(
		Guid key, UpdatePracticePlanRequest request)
	{
		var plan = await planRepo.GetDetailByKeyAsync(key)
			?? throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (plan.UserId != userContext.UserId)
			throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (request.Name is not null) plan.Name = request.Name;
		if (request.Location is not null) plan.Location = request.Location;
		if (request.IntendedDuration is not null) plan.IntendedDuration = request.IntendedDuration;
		if (request.Description is not null) plan.Description = request.Description;
		plan.LastModifiedAt = DateTime.UtcNow;

		await planRepo.UpdateAsync(plan);
		return plan.ToDetailResponse();
	}

	public async Task DeleteAsync(Guid key)
	{
		var plan = await planRepo.GetByKeyAsync(key)
			?? throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (plan.UserId != userContext.UserId)
			throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		await planRepo.DeleteAsync(plan);
	}
}
