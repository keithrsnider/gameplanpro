using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface ITeamRepository
{
	Task<Team?> GetByUserIdAsync(string userId);
	Task<Team> CreateAsync(Team team);
	Task UpdateAsync(Team team);
}

public class TeamRepository(AppDbContext db) : ITeamRepository
{
	public Task<Team?> GetByUserIdAsync(string userId)
	{
		return db.Teams
			.Include(t => t.Coaches)
			.FirstOrDefaultAsync(t => t.UserId == userId);
	}

	public async Task<Team> CreateAsync(Team team)
	{
		db.Teams.Add(team);
		await db.SaveChangesAsync();
		return team;
	}

	public async Task UpdateAsync(Team team)
	{
		db.Teams.Update(team);
		await db.SaveChangesAsync();
	}
}
