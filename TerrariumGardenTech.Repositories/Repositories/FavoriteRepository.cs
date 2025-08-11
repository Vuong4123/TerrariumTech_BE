using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class FavoriteRepository : GenericRepository<Favorite>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;

        public FavoriteRepository(TerrariumGardenTechDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Favorite> GetByUserAndAccessoryAsync(int userId, int accessoryId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.AccessoryId == accessoryId);
        }

        public async Task<Favorite> GetByUserAndTerrariumAsync(int userId, int terrariumId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TerrariumId == terrariumId);
        }

        public async Task<IEnumerable<Favorite>> GetByUserAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Accessory).ThenInclude(a => a.AccessoryImages)
                .Include(f => f.Terrarium).ThenInclude(t => t.TerrariumImages)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}
