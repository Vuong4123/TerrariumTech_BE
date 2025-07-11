using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TerrariumTankMethodRepository : GenericRepository<TerrariumTankMethod>
{
    public TerrariumGardenTechDBContext _dbContext;

    //public TerrariumTankMethodRepository() { }

    public TerrariumTankMethodRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;

    public async Task<List<TerrariumTankMethod>> GetAllTerrariumByTankMethods(int tankMethodId)
        {
            return await _dbContext
                .TerrariumTankMethods
                .AsNoTracking()
                .Include(te => te.Terrarium)
                .Where(te => te.TankMethodId == tankMethodId)
                .ToListAsync();
        }
    public async Task<List<TerrariumTankMethod>> GetTankMethodsByTerrariumId(int terrariumId)
    {
        return await _dbContext.TerrariumTankMethods
        .Where(tm => tm.TerrariumId == terrariumId).ToListAsync();
    }
}