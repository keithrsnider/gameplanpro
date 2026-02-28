using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface IDrillTypeRepository
{
	Task<List<DrillType>> GetAllAsync();
	Task<DrillType?> GetByKeyAsync(Guid key);
}

public class DrillTypeRepository(AppDbContext db) : IDrillTypeRepository
{
	public Task<List<DrillType>> GetAllAsync()
	{
		return db.DrillTypes.OrderBy(dt => dt.Name).ToListAsync();
	}

	public Task<DrillType?> GetByKeyAsync(Guid key)
	{
		return db.DrillTypes.FirstOrDefaultAsync(dt => dt.Key == key);
	}
}
