using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface ISectionRepository
{
	Task<Section?> GetByKeyAsync(Guid key);
	Task<List<Section>> GetByPracticePlanIdAsync(int practicePlanId);
	Task<Section> CreateAsync(Section section);
	Task UpdateAsync(Section section);
	Task UpdateRangeAsync(List<Section> sections);
	Task DeleteAsync(Section section);
}

public class SectionRepository(AppDbContext db) : ISectionRepository
{
	public Task<Section?> GetByKeyAsync(Guid key)
	{
		return db.Sections
			.Include(s => s.PlanDrills.OrderBy(pd => pd.DisplayOrder))
				.ThenInclude(pd => pd.DrillType)
			.FirstOrDefaultAsync(s => s.Key == key);
	}

	public Task<List<Section>> GetByPracticePlanIdAsync(int practicePlanId)
	{
		return db.Sections
			.Where(s => s.PracticePlanId == practicePlanId)
			.Include(s => s.PlanDrills.OrderBy(pd => pd.DisplayOrder))
				.ThenInclude(pd => pd.DrillType)
			.ToListAsync();
	}

	public async Task<Section> CreateAsync(Section section)
	{
		db.Sections.Add(section);
		await db.SaveChangesAsync();
		return section;
	}

	public Task UpdateAsync(Section section)
	{
		db.Sections.Update(section);
		return db.SaveChangesAsync();
	}

	public Task UpdateRangeAsync(List<Section> sections)
	{
		db.Sections.UpdateRange(sections);
		return db.SaveChangesAsync();
	}

	public Task DeleteAsync(Section section)
	{
		db.Sections.Remove(section);
		return db.SaveChangesAsync();
	}
}
