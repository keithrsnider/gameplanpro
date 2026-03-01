using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/practice-plans/{planKey:guid}/sections/{sectionKey:guid}/plan-drills")]
[Authorize]
public class PlanDrillsController(IPlanDrillService planDrillService) : ControllerBase
{
	[HttpPost]
	public async Task<ActionResult<PlanDrillResponse>> Create(
		Guid planKey, Guid sectionKey, [FromBody] CreatePlanDrillRequest request)
	{
		var drill = await planDrillService.CreateAsync(planKey, sectionKey, request);
		return Created(string.Empty, drill);
	}

	[HttpPut("{drillKey:guid}")]
	public Task<PlanDrillResponse> Update(
		Guid planKey, Guid sectionKey, Guid drillKey,
		[FromBody] UpdatePlanDrillRequest request)
	{
		return planDrillService.UpdateAsync(planKey, sectionKey, drillKey, request);
	}

	[HttpDelete("{drillKey:guid}")]
	public async Task<IActionResult> Delete(Guid planKey, Guid sectionKey, Guid drillKey)
	{
		await planDrillService.DeleteAsync(planKey, sectionKey, drillKey);
		return NoContent();
	}
}
