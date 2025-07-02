using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TerrariumTankMethodRepository : GenericRepository<TerrariumTankMethod>
{
    public TerrariumGardenTechDBContext _dbContext;

    //public TerrariumTankMethodRepository() { }

    public TerrariumTankMethodRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;

    // Additional methods specific to TerrariumTankMethod can be added here
}