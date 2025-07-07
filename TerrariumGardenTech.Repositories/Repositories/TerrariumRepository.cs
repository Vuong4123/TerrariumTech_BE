using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class TerrariumRepository : GenericRepository<Terrarium>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public TerrariumRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;

        public async Task<List<Terrarium>> GetAllTerrariumAsync()
        {
            return await _dbContext
            .Terrariums
            .AsNoTracking()
            .Include(t => t.TerrariumImages)
            .Include(t => t.TerrariumShapes)
                .ThenInclude(ts => ts.Shape)
            .Include(t => t.TerrariumEnvironments)
                .ThenInclude(te => te.EnvironmentTerrarium)
            .Include(t => t.TerrariumTankMethods)
                .ThenInclude(ttm => ttm.TankMethod)
            .Include(t => t.TerrariumAccessory)
                .ThenInclude(ta => ta.Accessory)
            .ToListAsync();
        }
        public async Task<Terrarium?> GetTerrariumIdAsync(int id)
        {
            return await _dbContext
                .Terrariums
                .AsNoTracking()
                .Include(t => t.TerrariumImages)
                .Include(ta => ta.TerrariumShapes)
                    .ThenInclude(s => s.Shape)
                .Include(ta => ta.TerrariumEnvironments)
                    .ThenInclude(e => e.EnvironmentTerrarium)
                .Include(ta => ta.TerrariumTankMethods)
                    .ThenInclude(t => t.TankMethod)
                .Include(ta => ta.TerrariumAccessory)
                    .ThenInclude(ta => ta.Accessory)
                .FirstOrDefaultAsync(ta => ta.TerrariumId == id);

        }
        public async Task<List<Terrarium>> GetTerrariumByIdsAsync(List<int> terrariumIds)
        {
            return await _dbContext
            .Terrariums
            .Where(t => terrariumIds.Contains(t.TerrariumId)).ToListAsync();
        }
    }
}
