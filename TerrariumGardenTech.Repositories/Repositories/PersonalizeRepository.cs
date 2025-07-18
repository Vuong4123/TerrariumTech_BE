using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class PersonalizeRepository : GenericRepository<Personalize>
{
    private readonly TerrariumGardenTechDBContext _dbContexxt;

    public PersonalizeRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContexxt = dbContext;
    }
}