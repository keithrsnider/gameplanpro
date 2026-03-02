using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/team")]
[Authorize]
public class TeamController(ITeamService teamService) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<TeamResponse>> Get()
	{
		var team = await teamService.GetAsync();
		if (team is null) return NotFound();
		return team;
	}

	[HttpPost]
	public Task<TeamResponse> Create([FromBody] CreateTeamRequest request)
	{
		return teamService.CreateAsync(request);
	}

	[HttpPut]
	public Task<TeamResponse> Update([FromBody] UpdateTeamRequest request)
	{
		return teamService.UpdateAsync(request);
	}
}
