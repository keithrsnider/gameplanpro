using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/practice-plans")]
[Authorize]
public class PracticePlansController(IPracticePlanService planService) : ControllerBase
{
	[HttpGet]
	public Task<List<PracticePlanListResponse>> GetAll()
	{
		return planService.GetAllAsync();
	}

	[HttpGet("{key:guid}")]
	public Task<PracticePlanDetailResponse> Get(Guid key)
	{
		return planService.GetByKeyAsync(key);
	}

	[HttpPost]
	public async Task<ActionResult<PracticePlanDetailResponse>> Create(
		[FromBody] CreatePracticePlanRequest request)
	{
		var plan = await planService.CreateAsync(request);
		return CreatedAtAction(nameof(Get), new { key = plan.Key }, plan);
	}

	[HttpPut("{key:guid}")]
	public Task<PracticePlanDetailResponse> Update(
		Guid key, [FromBody] UpdatePracticePlanRequest request)
	{
		return planService.UpdateAsync(key, request);
	}

	[HttpDelete("{key:guid}")]
	public async Task<IActionResult> Delete(Guid key)
	{
		await planService.DeleteAsync(key);
		return NoContent();
	}
}
