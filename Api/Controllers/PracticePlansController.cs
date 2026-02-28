using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/practice-plans")]
[Authorize]
public class PracticePlansController(IPracticePlanService planService) : ControllerBase
{
	[HttpGet]
	public Task<List<PracticePlanListResponse>> GetAll()
	{
		return planService.GetAllAsync(GetUserId());
	}

	[HttpGet("{key:guid}")]
	public Task<PracticePlanDetailResponse> Get(Guid key)
	{
		return planService.GetByKeyAsync(GetUserId(), key);
	}

	[HttpPost]
	public async Task<ActionResult<PracticePlanDetailResponse>> Create(
		[FromBody] CreatePracticePlanRequest request)
	{
		var plan = await planService.CreateAsync(GetUserId(), request);
		return CreatedAtAction(nameof(Get), new { key = plan.Key }, plan);
	}

	[HttpPut("{key:guid}")]
	public Task<PracticePlanDetailResponse> Update(
		Guid key, [FromBody] UpdatePracticePlanRequest request)
	{
		return planService.UpdateAsync(GetUserId(), key, request);
	}

	[HttpDelete("{key:guid}")]
	public async Task<IActionResult> Delete(Guid key)
	{
		await planService.DeleteAsync(GetUserId(), key);
		return NoContent();
	}

	private string GetUserId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
	}
}
