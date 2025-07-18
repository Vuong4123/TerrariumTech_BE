using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class AccessoryImageRepository : GenericRepository<AccessoryImage>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public AccessoryImageRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AccessoryImage>> GetAllByAccessoryIdAsync(int accessoryId)
    {
        return await _context.AccessoryImages
            .Where(ti => ti.AccessoryId == accessoryId)
            .ToListAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<AccessoryImage> entities)
    {
        _context.AccessoryImages.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }
}