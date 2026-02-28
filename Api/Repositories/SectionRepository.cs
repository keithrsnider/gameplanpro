using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface ISectionRepository
{
	Task<Section?> GetByKeyAsync(Guid key);
	Task<Section> CreateAsync(Section section);
	Task UpdateAsync(Section section);
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

	public Task DeleteAsync(Section section)
	{
		db.Sections.Remove(section);
		return db.SaveChangesAsync();
	}
}
