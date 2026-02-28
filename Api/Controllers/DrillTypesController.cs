using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/drill-types")]
[Authorize]
public class DrillTypesController(IDrillTypeService drillTypeService) : ControllerBase
{
	[HttpGet]
	public Task<List<DrillTypeResponse>> GetAll()
	{
		return drillTypeService.GetAllAsync();
	}
}
