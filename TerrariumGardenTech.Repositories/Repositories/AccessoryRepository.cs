using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class AccessoryRepository : GenericRepository<Accessory>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        //public AccessoryRepository() { }
        public AccessoryRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
        // Add any specific methods for AccessoryRepository here
    }
    
}
