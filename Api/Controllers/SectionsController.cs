using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/practice-plans/{planKey:guid}/sections")]
[Authorize]
public class SectionsController(ISectionService sectionService) : ControllerBase
{
	[HttpPost]
	public async Task<ActionResult<SectionResponse>> Create(
		Guid planKey, [FromBody] CreateSectionRequest request)
	{
		var section = await sectionService.CreateAsync(GetUserId(), planKey, request);
		return Created(string.Empty, section);
	}

	[HttpPut("{sectionKey:guid}")]
	public Task<SectionResponse> Update(
		Guid planKey, Guid sectionKey, [FromBody] UpdateSectionRequest request)
	{
		return sectionService.UpdateAsync(GetUserId(), planKey, sectionKey, request);
	}

	[HttpDelete("{sectionKey:guid}")]
	public async Task<IActionResult> Delete(Guid planKey, Guid sectionKey)
	{
		await sectionService.DeleteAsync(GetUserId(), planKey, sectionKey);
		return NoContent();
	}

	private string GetUserId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
	}
}
