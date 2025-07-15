using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class AccessoryShapeRepository : GenericRepository<AccessoryShape>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public AccessoryShapeRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
        public async Task<List<AccessoryShape>> GetAllAccessoryByShapes(int shapeId)
        {
            return await _dbContext
                .AccessoryShapes
                .AsNoTracking()
                .Include(te => te.Accessory)
                .Where(te => te.ShapeId == shapeId)
                .ToListAsync();
        }
        public async Task<List<AccessoryShape>> GetAccessoryShapesByAccessoryIdAsync(int terrariumId)
        {
            return await _dbContext.AccessoryShapes
                          .Where(ts => ts.AccessoryId == terrariumId)
                          .ToListAsync();
        }
    }
}
