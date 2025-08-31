using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TerrariumVariantRepository : GenericRepository<TerrariumVariant>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public TerrariumVariantRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<TerrariumVariant>> GetAllAsync2()
    {
        return _context.TerrariumVariants.Include(c => c.TerrariumVariantAccessories).ToList();
    }
    public async Task<IEnumerable<TerrariumVariant>> GetAllByTerrariumIdAsync(int terrariumId)
    {
        return await _context.TerrariumVariants
            .Include(a => a.TerrariumVariantAccessories)
            .Where(ti => ti.TerrariumId == terrariumId)
            .ToListAsync();
    }
    public async Task RemoveRangeAsync(IEnumerable<TerrariumVariant> entities)
    {
        _context.TerrariumVariants.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }
    public async Task<TerrariumVariant> GetByIdAsync2(int id)
    {
        return _context.TerrariumVariants.Include(c => c.TerrariumVariantAccessories).FirstOrDefault(x => x.TerrariumVariantId == id);
    }
}