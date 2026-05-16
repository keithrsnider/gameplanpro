using Api.Models.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/coaches")]
[Authorize]
public class CoachController(ICoachService coachService) : ControllerBase
{
	[HttpGet("by-team")]
	public Task<List<CoachResponse>> GetByTeam()
	{
		return coachService.GetByTeamAsync();
	}
}

