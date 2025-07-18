using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TerrariumAccessoryRepository : GenericRepository<TerrariumAccessory>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public TerrariumAccessoryRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TerrariumAccessory>> GetAllTerrariumByAccessory(int accessoryId)
    {
        return await _dbContext
            .TerrariumAccessory
            .AsNoTracking()
            .Include(te => te.Terrarium)
            .Where(te => te.AccessoryId == accessoryId)
            .ToListAsync();
    }

    public async Task<List<TerrariumAccessory>> GetTerrariumAccessoriesByTerrariumAsync(int terrariumId)
    {
        return await _dbContext.TerrariumAccessory.Where(ta => ta.TerrariumId == terrariumId).ToListAsync();
    }
}