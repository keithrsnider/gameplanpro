using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface IPracticePlanRepository
{
	Task<List<PracticePlan>> GetAllByUserAsync(string userId);
	Task<PracticePlan?> GetByKeyAsync(Guid key);
	Task<PracticePlan?> GetDetailByKeyAsync(Guid key);
	Task<PracticePlan> CreateAsync(PracticePlan plan);
	Task UpdateAsync(PracticePlan plan);
	Task DeleteAsync(PracticePlan plan);
}

public class PracticePlanRepository(AppDbContext db) : IPracticePlanRepository
{
	public Task<List<PracticePlan>> GetAllByUserAsync(string userId)
	{
		return db.PracticePlans
			.Where(pp => pp.UserId == userId)
			.OrderByDescending(pp => pp.LastModifiedAt)
			.ToListAsync();
	}

	public Task<PracticePlan?> GetByKeyAsync(Guid key)
	{
		return db.PracticePlans.FirstOrDefaultAsync(pp => pp.Key == key);
	}

	public Task<PracticePlan?> GetDetailByKeyAsync(Guid key)
	{
		return db.PracticePlans
			.Include(pp => pp.Sections.OrderBy(s => s.DisplayOrder))
				.ThenInclude(s => s.PlanDrills.OrderBy(pd => pd.DisplayOrder))
					.ThenInclude(pd => pd.DrillType)
			.FirstOrDefaultAsync(pp => pp.Key == key);
	}

	public async Task<PracticePlan> CreateAsync(PracticePlan plan)
	{
		db.PracticePlans.Add(plan);
		await db.SaveChangesAsync();
		return plan;
	}

	public Task UpdateAsync(PracticePlan plan)
	{
		db.PracticePlans.Update(plan);
		return db.SaveChangesAsync();
	}

	public Task DeleteAsync(PracticePlan plan)
	{
		db.PracticePlans.Remove(plan);
		return db.SaveChangesAsync();
	}
}
