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
            .Include(t => t.TerrariumShapes)
                .ThenInclude(ts => ts.Shape)
            .Include(t => t.TerrariumEnvironments)
                .ThenInclude(te => te.Environment)
            .Include(t => t.TerrariumTankMethods)
                .ThenInclude(ttm => ttm.TankMethod)
            .ToListAsync();
        }
        public async Task<Terrarium?> GetTerrariumIdAsync(int id)
        {
            return await _dbContext
                   .Terrariums
                   .AsNoTracking()
                   .Include(ta => ta.TerrariumShapes)
                    .ThenInclude(s => s.Shape)
                    .Include(ta => ta.TerrariumEnvironments)
                    .ThenInclude(e => e.Environment)
                    .Include(ta => ta.TerrariumTankMethods)
                    .ThenInclude(t => t.TankMethod)
                    .FirstOrDefaultAsync(ta => ta.TerrariumId == id);

        }
    }
}
