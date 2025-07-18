using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using Environment = TerrariumGardenTech.Repositories.Entity.EnvironmentTerrarium;

namespace TerrariumGardenTech.Repositories.Repositories;

public class EnvironmentRepository : GenericRepository<Environment>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    //public EnvironmentRepository() { }
    public EnvironmentRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<Environment?> GetByName(string? name)
    {
        return await _dbContext.Set<Environment>().FirstOrDefaultAsync(e => e.EnvironmentName == name);
    }
}