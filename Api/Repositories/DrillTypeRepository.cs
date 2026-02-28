using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface IDrillTypeRepository
{
	Task<List<DrillType>> GetAllAsync();
	Task<DrillType?> GetByIdAsync(int id);
}

public class DrillTypeRepository(AppDbContext db) : IDrillTypeRepository
{
	public Task<List<DrillType>> GetAllAsync()
	{
		return db.DrillTypes.OrderBy(dt => dt.Name).ToListAsync();
	}

	public Task<DrillType?> GetByIdAsync(int id)
	{
		return db.DrillTypes.FirstOrDefaultAsync(dt => dt.Id == id);
	}
}
