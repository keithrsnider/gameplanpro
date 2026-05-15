using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface IDrillRepository
{
	Task<List<Drill>> GetAllAsync(string? userId, DrillSource? source, int? drillTypeId);
	Task<Drill?> GetByKeyAsync(Guid key);
	Task<Drill> CreateAsync(Drill drill);
	Task UpdateAsync(Drill drill);
	Task DeleteAsync(Drill drill);
}

public class DrillRepository(AppDbContext db) : IDrillRepository
{
	public Task<List<Drill>> GetAllAsync(string? userId, DrillSource? source, int? drillTypeId)
	{
		var query = db.Drills
			.Include(d => d.DrillType)
			.Include(d => d.Coach)
			.AsQueryable();

		if (source is not null)
			query = query.Where(d => d.Source == source);

		if (drillTypeId is not null)
			query = query.Where(d => d.DrillTypeId == drillTypeId);

		// User drills: show only the user's own. System drills: show all.
		if (source == DrillSource.User && userId is not null)
			query = query.Where(d => d.UserId == userId);

		return query.OrderBy(d => d.Name).ToListAsync();
	}

	public Task<Drill?> GetByKeyAsync(Guid key)
	{
		return db.Drills
			.Include(d => d.DrillType)
			.Include(d => d.Coach)
			.FirstOrDefaultAsync(d => d.Key == key);
	}

	public async Task<Drill> CreateAsync(Drill drill)
	{
		db.Drills.Add(drill);
		await db.SaveChangesAsync();
		return drill;
	}

	public Task UpdateAsync(Drill drill)
	{
		db.Drills.Update(drill);
		return db.SaveChangesAsync();
	}

	public Task DeleteAsync(Drill drill)
	{
		db.Drills.Remove(drill);
		return db.SaveChangesAsync();
	}
}
