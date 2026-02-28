using Api.Models;
using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/drills")]
[Authorize]
public class DrillsController(IDrillService drillService) : ControllerBase
{
	[HttpGet]
	public Task<List<DrillResponse>> GetAll(
		[FromQuery] DrillSource? source,
		[FromQuery] Guid? drillTypeKey)
	{
		return drillService.GetAllAsync(GetUserId(), source, drillTypeKey);
	}

	[HttpGet("{key:guid}")]
	public Task<DrillResponse> Get(Guid key)
	{
		return drillService.GetByKeyAsync(GetUserId(), key);
	}

	[HttpPost]
	public async Task<ActionResult<DrillResponse>> Create([FromBody] CreateDrillRequest request)
	{
		var drill = await drillService.CreateAsync(GetUserId(), request);
		return CreatedAtAction(nameof(Get), new { key = drill.Key }, drill);
	}

	[HttpPut("{key:guid}")]
	public Task<DrillResponse> Update(Guid key, [FromBody] UpdateDrillRequest request)
	{
		return drillService.UpdateAsync(GetUserId(), key, request);
	}

	[HttpDelete("{key:guid}")]
	public async Task<IActionResult> Delete(Guid key)
	{
		await drillService.DeleteAsync(GetUserId(), key);
		return NoContent();
	}

	private string GetUserId()
	{
		return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
	}
}
