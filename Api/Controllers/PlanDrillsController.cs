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
	public Task<PlanDrillResponse> Create(
		Guid planKey, Guid sectionKey, [FromBody] CreatePlanDrillRequest request)
	{
		return planDrillService.CreateAsync(planKey, sectionKey, request);
	}

	[HttpPut("{drillKey:guid}")]
	public Task<PlanDrillResponse> Update(
		Guid planKey, Guid sectionKey, Guid drillKey,
		[FromBody] UpdatePlanDrillRequest request)
	{
		return planDrillService.UpdateAsync(planKey, sectionKey, drillKey, request);
	}

	[HttpDelete("{drillKey:guid}")]
	public Task Delete(Guid planKey, Guid sectionKey, Guid drillKey)
	{
		return planDrillService.DeleteAsync(planKey, sectionKey, drillKey);
	}
}
