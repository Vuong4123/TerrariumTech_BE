
using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

public class TerrariumEnvironmentRepository : GenericRepository<TerrariumEnvironment>
{
    public TerrariumGardenTechDBContext _dbContext;

    //public TerrariumEnvironmentRepository() { }

    public TerrariumEnvironmentRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;

    // Additional methods specific to TerrariumEnvironment can be added here
    public async Task<List<TerrariumEnvironment>> GetAllTerrariumByEnvironment(int environmentId)
    {
        return await _dbContext
            .TerrariumEnvironments
            .AsNoTracking()
            .Include(te => te.Terrarium)
            .Where(te => te.EnvironmentId == environmentId)
            .ToListAsync();
    }

    public async Task<List<TerrariumShape>> GetTerrariumEnvironmentByTerrariumIdAsync(int terrariumId)
        {
            return await _dbContext.TerrariumShapes
                          .Where(ts => ts.TerrariumId == terrariumId)
                          .ToListAsync();
        }

}