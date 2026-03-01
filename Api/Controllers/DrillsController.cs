using Api.Models;
using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/drills")]
[Authorize]
public class DrillsController(IDrillService drillService) : ControllerBase
{
	[HttpGet]
	public Task<List<DrillResponse>> GetAll(
		[FromQuery] DrillSource? source,
		[FromQuery] int? drillTypeId)
	{
		return drillService.GetAllAsync(source, drillTypeId);
	}

	[HttpGet("{key:guid}")]
	public Task<DrillResponse> Get(Guid key)
	{
		return drillService.GetByKeyAsync(key);
	}

	[HttpPost]
	public async Task<ActionResult<DrillResponse>> Create([FromBody] CreateDrillRequest request)
	{
		var drill = await drillService.CreateAsync(request);
		return CreatedAtAction(nameof(Get), new { key = drill.Key }, drill);
	}

	[HttpPut("{key:guid}")]
	public Task<DrillResponse> Update(Guid key, [FromBody] UpdateDrillRequest request)
	{
		return drillService.UpdateAsync(key, request);
	}

	[HttpDelete("{key:guid}")]
	public async Task<IActionResult> Delete(Guid key)
	{
		await drillService.DeleteAsync(key);
		return NoContent();
	}
}
