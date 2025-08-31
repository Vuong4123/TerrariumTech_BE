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

    public async Task<List<TerrariumVariantAccessory>> GetByTerrariumVariantIdAsync(int terrariumVariantId)
    {
        return await _db.TerrariumVariantAccessory
            .Include(tva => tva.Accessory)
            .Where(x => x.TerrariumVariantId == terrariumVariantId)
            .ToListAsync();
    }

    public async Task<List<TerrariumVariantAccessory>> GetByAccessoryIdAsync(int accessoryId)
    {
        return await _db.TerrariumVariantAccessory
            .Include(tva => tva.TerrariumVariant)
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