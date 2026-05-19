using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/practice-plans/{planKey:guid}/sections")]
[Authorize]
public class SectionsController(ISectionService sectionService) : ControllerBase
{
	[HttpPost]
	public Task<SectionResponse> Create(
		Guid planKey, [FromBody] CreateSectionRequest request)
	{
		return sectionService.CreateAsync(planKey, request);
	}

	[HttpPut("{sectionKey:guid}")]
	public Task<SectionResponse> Update(
		Guid planKey, Guid sectionKey, [FromBody] UpdateSectionRequest request)
	{
		return sectionService.UpdateAsync(planKey, sectionKey, request);
	}

	[HttpPut("order")]
	public Task<List<SectionResponse>> BulkUpdateOrder(
		Guid planKey, [FromBody] List<BulkUpdateSectionDisplayOrderRequest> request)
	{
		return sectionService.BulkUpdateOrderAsync(planKey, request);
	}

	[HttpDelete("{sectionKey:guid}")]
	public Task Delete(Guid planKey, Guid sectionKey)
	{
		return sectionService.DeleteAsync(planKey, sectionKey);
	}
}
