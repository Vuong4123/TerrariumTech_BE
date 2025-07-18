using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class RoleRepository : GenericRepository<Role>
{
    public TerrariumGardenTechDBContext _dbContext;

    //public RoleRepository() { }
    public RoleRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }
}