using Api.Models;
using Api.Models.Dtos;
using Api.Models.Mappers;
using Api.Repositories;

namespace Api.Services;

public interface ISectionService
{
	Task<SectionResponse> CreateAsync(Guid planKey, CreateSectionRequest request);
	Task<SectionResponse> UpdateAsync(
		Guid planKey, Guid sectionKey, UpdateSectionRequest request
	);
	Task<List<SectionResponse>> BulkUpdateOrderAsync(
		Guid planKey, List<BulkUpdateSectionDisplayOrderRequest> request
	);
	Task DeleteAsync(Guid planKey, Guid sectionKey);
}

public class SectionService(
	ISectionRepository sectionRepo,
	IPracticePlanRepository planRepo,
	IUserContext userContext
) : ISectionService
{
	public async Task<SectionResponse> CreateAsync(
		Guid planKey, CreateSectionRequest request)
	{
		var plan = await planRepo.GetByKeyAsync(planKey)
			?? throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (plan.UserId != userContext.UserId)
			throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		var section = new Section
		{
			Name = request.Name,
			DisplayOrder = request.DisplayOrder,
			PracticePlanId = plan.Id,
		};

		await sectionRepo.CreateAsync(section);
		plan.LastModifiedAt = DateTime.UtcNow;
		await planRepo.UpdateAsync(plan);

		return new SectionResponse(section.Key, section.Name, section.DisplayOrder, section.Note, []);
	}

	public async Task<SectionResponse> UpdateAsync(
		Guid planKey, Guid sectionKey, UpdateSectionRequest request)
	{
		var plan = await planRepo.GetByKeyAsync(planKey)
			?? throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (plan.UserId != userContext.UserId)
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

		if (request.Name is not null) section.Name = request.Name;
		if (request.DisplayOrder is not null) section.DisplayOrder = request.DisplayOrder.Value;
		if (request.Note is not null) section.Note = request.Note;

		await sectionRepo.UpdateAsync(section);
		plan.LastModifiedAt = DateTime.UtcNow;
		await planRepo.UpdateAsync(plan);

		return section.ToResponse();
	}

	public async Task<List<SectionResponse>> BulkUpdateOrderAsync(
		Guid planKey, List<BulkUpdateSectionDisplayOrderRequest> request)
	{
		var plan = await planRepo.GetByKeyAsync(planKey)
			?? throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (plan.UserId != userContext.UserId)
			throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		var sections = await sectionRepo.GetByPracticePlanIdAsync(plan.Id);
		var sectionCount = sections.Count;

		if (request.Count != sectionCount)
			throw new BadHttpRequestException(
				$"All {sectionCount} sections must be provided.",
				StatusCodes.Status400BadRequest
			);

		var duplicateSectionKey = request
			.GroupBy(r => r.SectionKey)
			.FirstOrDefault(g => g.Count() > 1);
		if (duplicateSectionKey is not null)
			throw new BadHttpRequestException(
				"Section keys must be unique.",
				StatusCodes.Status400BadRequest
			);

		var sectionsByKey = sections.ToDictionary(s => s.Key);
		var invalidSectionKey = request.FirstOrDefault(r => !sectionsByKey.ContainsKey(r.SectionKey));
		if (invalidSectionKey is not null)
			throw new BadHttpRequestException(
				"One or more section keys do not belong to this practice plan.",
				StatusCodes.Status400BadRequest
			);

		if (request.Any(r => r.DisplayOrder < 1 || r.DisplayOrder > sectionCount))
			throw new BadHttpRequestException(
				$"Display order must be in the range 1-{sectionCount}.",
				StatusCodes.Status400BadRequest
			);

		var displayOrderSet = request.Select(r => r.DisplayOrder).ToHashSet();
		if (displayOrderSet.Count != sectionCount)
			throw new BadHttpRequestException(
				"Display order values must be unique.",
				StatusCodes.Status400BadRequest
			);

		for (var expectedDisplayOrder = 1; expectedDisplayOrder <= sectionCount; expectedDisplayOrder++)
		{
			if (!displayOrderSet.Contains(expectedDisplayOrder))
				throw new BadHttpRequestException(
					$"Display order must contain all values from 1-{sectionCount}.",
					StatusCodes.Status400BadRequest
				);
		}

		foreach (var item in request)
		{
			var section = sectionsByKey[item.SectionKey];
			section.DisplayOrder = item.DisplayOrder;
		}

		await sectionRepo.UpdateRangeAsync(sections);
		plan.LastModifiedAt = DateTime.UtcNow;
		await planRepo.UpdateAsync(plan);

		return sections
			.OrderBy(s => s.DisplayOrder)
			.Select(s => s.ToResponse())
			.ToList();
	}

	public async Task DeleteAsync(Guid planKey, Guid sectionKey)
	{
		var plan = await planRepo.GetByKeyAsync(planKey)
			?? throw new BadHttpRequestException(
				"Practice plan not found.", StatusCodes.Status404NotFound
			);

		if (plan.UserId != userContext.UserId)
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

		await sectionRepo.DeleteAsync(section);
		plan.LastModifiedAt = DateTime.UtcNow;
		await planRepo.UpdateAsync(plan);
	}
}
