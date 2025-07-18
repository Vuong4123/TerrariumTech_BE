using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class UserRepository : GenericRepository<User>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public UserRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }
}