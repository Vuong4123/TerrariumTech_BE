using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class TerrariumRepository : GenericRepository<Terrarium>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public TerrariumRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;

        public async Task<IEnumerable<Terrarium>> FilterTerrariumsAsync(int? environmentId, int? shapeId, int? tankMethodId)
        {
            var query = _context.Terrariums.AsQueryable();

            if (environmentId.HasValue)
            {
                query = query.Where(t => t.EnvironmentId == environmentId.Value);
            }

            if (shapeId.HasValue)
            {
                query = query.Where(t => t.ShapeId == shapeId.Value);
            }

            if (tankMethodId.HasValue)
            {
                query = query.Where(t => t.TankMethodId == tankMethodId.Value);
            }

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Terrarium>> GetAllByTankMethodIdAsync(int tankMethodId)
        {
            return await _context.Terrariums
                                 .Where(t => t.TankMethodId == tankMethodId)
                                 .ToListAsync();
        }
        public async Task<IEnumerable<Terrarium>> GetAllByShapeIdAsync(int shapeId)
        {
            return await _context.Terrariums
                                 .Where(t => t.ShapeId == shapeId)
                                 .ToListAsync();
        }
        public async Task<IEnumerable<Terrarium>> GetAllByEnvironmentIdAsync(int environmentId)
        {
            return await _context.Terrariums
                                 .Where(t => t.EnvironmentId == environmentId)
                                 .ToListAsync();
        }
        public async Task<List<Terrarium>> GetTerrariumByIdsAsync(List<int> terrariumIds)
        {
            return await _dbContext
            .Terrariums
            .Where(t => terrariumIds.Contains(t.TerrariumId)).ToListAsync();
        }
    }
}
