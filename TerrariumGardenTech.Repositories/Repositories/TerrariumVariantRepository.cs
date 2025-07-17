using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class TerrariumVariantRepository : GenericRepository<TerrariumVariant>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public TerrariumVariantRepository(TerrariumGardenTechDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<TerrariumVariant>> GetAllByTerrariumIdAsync(int terrariumId)
        {
            return await _context.TerrariumVariants
                .Where(ti => ti.TerrariumId == terrariumId)
                .ToListAsync();
        }
        public async Task RemoveRangeAsync(IEnumerable<TerrariumVariant> entities)
        {
            _context.TerrariumVariants.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }
    }
}
