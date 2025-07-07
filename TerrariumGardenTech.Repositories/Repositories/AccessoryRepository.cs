using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Accessory?>> GetByName(List<string?> name)
        {
            return await _dbContext.Set<Accessory>()
                               .Where(e => name.Contains(e.Name))  // Tìm tất cả các Accessory có tên nằm trong danh sách names
                               .ToListAsync();
        }
    }

}
