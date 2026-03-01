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
	public Task<DrillResponse> Create([FromBody] CreateDrillRequest request)
	{
		return drillService.CreateAsync(request);
	}

	[HttpPut("{key:guid}")]
	public Task<DrillResponse> Update(Guid key, [FromBody] UpdateDrillRequest request)
	{
		return drillService.UpdateAsync(key, request);
	}

	[HttpDelete("{key:guid}")]
	public Task Delete(Guid key)
	{
		return drillService.DeleteAsync(key);
	}
}
