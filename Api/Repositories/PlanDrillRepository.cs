using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface IPlanDrillRepository
{
	Task<PlanDrill?> GetByKeyAsync(Guid key);
	Task<PlanDrill> CreateAsync(PlanDrill planDrill);
	Task UpdateAsync(PlanDrill planDrill);
	Task DeleteAsync(PlanDrill planDrill);
}

public class PlanDrillRepository(AppDbContext db) : IPlanDrillRepository
{
	public Task<PlanDrill?> GetByKeyAsync(Guid key)
	{
		return db.PlanDrills
			.Include(pd => pd.DrillType)
			.Include(pd => pd.Section)
			.FirstOrDefaultAsync(pd => pd.Key == key);
	}

	public async Task<PlanDrill> CreateAsync(PlanDrill planDrill)
	{
		db.PlanDrills.Add(planDrill);
		await db.SaveChangesAsync();
		return planDrill;
	}

	public Task UpdateAsync(PlanDrill planDrill)
	{
		db.PlanDrills.Update(planDrill);
		return db.SaveChangesAsync();
	}

	public Task DeleteAsync(PlanDrill planDrill)
	{
		db.PlanDrills.Remove(planDrill);
		return db.SaveChangesAsync();
	}
}
