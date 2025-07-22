using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TankMethodRepository : GenericRepository<TankMethod>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    //public TankMethodRepository() { }
    public TankMethodRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TankMethod?> GetByName(string? name)
    {
        return await _dbContext.Set<TankMethod>().FirstOrDefaultAsync(t => t.TankMethodType == name);
    }
}