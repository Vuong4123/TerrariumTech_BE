using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class PersonnalizeRepository : GenericRepository<Personalize>
    {
       
       private readonly TerrariumGardenTechDBContext _dbContexxt;
       public PersonnalizeRepository(TerrariumGardenTechDBContext dbContext) => _dbContexxt = dbContext;
    }
}
