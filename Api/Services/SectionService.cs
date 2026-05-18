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
