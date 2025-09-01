using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TerrariumVariantAccessoryRepository : GenericRepository<TerrariumVariantAccessory>
{
    private readonly TerrariumGardenTechDBContext _db;
   
    public TerrariumVariantAccessoryRepository(TerrariumGardenTechDBContext dbContext) : base(dbContext)
    {
        _db = dbContext;
    }
    // ✅ HÀM MỚI - GET ACCESSORIES BY VARIANT ID
    public async Task<List<TerrariumVariantAccessory>> GetByTerrariumVariantId(int terrariumVariantId)
    {
        return await _context.TerrariumVariantAccessory
            .Include(tva => tva.Accessory) // Include accessory details
            .Where(tva => tva.TerrariumVariantId == terrariumVariantId)
            .ToListAsync();
    }

    // ✅ DELETE ALL ACCESSORIES FOR A VARIANT
    public async Task DeleteByVariantIdAsync(int terrariumVariantId)
    {
        var accessories = await _context.TerrariumVariantAccessory
            .Where(tva => tva.TerrariumVariantId == terrariumVariantId)
            .ToListAsync();

        _context.TerrariumVariantAccessory.RemoveRange(accessories);
        await _context.SaveChangesAsync();
    }

    // ✅ CREATE MULTIPLE ACCESSORIES FOR VARIANT
    public async Task CreateMultipleAsync(List<TerrariumVariantAccessory> accessories)
    {
        _context.TerrariumVariantAccessory.AddRange(accessories);
        await _context.SaveChangesAsync();
    }

    // ✅ CHECK IF ACCESSORY IS USED IN VARIANT
    public async Task<bool> ExistsAsync(int terrariumVariantId, int accessoryId)
    {
        return await _context.TerrariumVariantAccessory
            .AnyAsync(tva => tva.TerrariumVariantId == terrariumVariantId && tva.AccessoryId == accessoryId);
    }
    public async Task<List<TerrariumVariant>> GetByTerrariumVariantIdAsync(int terrariumVariantId)
    {
        return await _db.TerrariumVariants
            .Include(tva => tva.TerrariumVariantAccessories)
            .ThenInclude(c => c.Accessory)
            .Where(x => x.TerrariumVariantId == terrariumVariantId)
            .ToListAsync();
    }

    public async Task<List<TerrariumVariantAccessory>> GetByAccessoryIdAsync(int accessoryId)
    {
        return await _db.TerrariumVariantAccessory
            .Include(tva => tva.TerrariumVariant)
            .Include(c => c.Accessory)
            .Where(x => x.AccessoryId == accessoryId)
            .ToListAsync();
    }

    public async Task RemoveByTerrariumVariantIdAsync(int terrariumVariantId)
    {
        var items = await _db.TerrariumVariantAccessory
            .Where(x => x.TerrariumVariantId == terrariumVariantId)
            .ToListAsync();

        _db.TerrariumVariantAccessory.RemoveRange(items);
        await _db.SaveChangesAsync();
    }
}