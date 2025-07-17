using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class TerrariumImageRepository : GenericRepository<TerrariumImage>
    {
        public TerrariumGardenTechDBContext _dbContext;
        //public TerrariumImageRepository() { }

        public TerrariumImageRepository(TerrariumGardenTechDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TerrariumImage>> GetAllByTerrariumIdAsync(int terrariumId)
        {
            return await _context.TerrariumImages
                .Where(ti => ti.TerrariumId == terrariumId)
                .ToListAsync();
        }
        public async Task RemoveRangeAsync(IEnumerable<TerrariumImage> entities)
        {
            _context.TerrariumImages.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }
    }
}
