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
        return _context.TerrariumVariants.Include(c => c.TerrariumVariantAccessories)
            .ThenInclude(a => a.Accessory)
            .ToList();
    }
    public async Task RestoreStockAsync(int variantId, int quantity)
    {
        var affected = await _dbContext.TerrariumVariants.Include(c => c.TerrariumVariantAccessories)
            .ThenInclude(va => va.Accessory)
            .Where(x => x.TerrariumVariantId == variantId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.StockQuantity, x => x.StockQuantity + quantity));

        if (affected == 0)
            throw new InvalidOperationException($"Không thể hoàn stock terrarium variant {variantId}");
    }
    public async Task<IEnumerable<TerrariumVariant>> GetAllByTerrariumIdAsync(int terrariumId)
    {
        return await _context.TerrariumVariants
            .Include(a => a.TerrariumVariantAccessories).ThenInclude(va => va.Accessory)
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
        return _context.TerrariumVariants.Include(c => c.TerrariumVariantAccessories).ThenInclude(va => va.Accessory).FirstOrDefault(x => x.TerrariumVariantId == id);
    }
}