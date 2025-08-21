using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TerrariumLayoutRepository : GenericRepository<TerrariumLayout>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public TerrariumLayoutRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<TerrariumLayout?> GetWithFullDetailsAsync(int id)
    {
        return await _context.TerrariumLayouts
            .Include(l => l.User)
            .Include(l => l.Reviewer)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.Environment)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.Shapes)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.TankMethods)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.TerrariumImages)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.TerrariumAccessory)
                    .ThenInclude(ta => ta.Accessory)
                        .ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(l => l.LayoutId == id);
    }

    public async Task<List<TerrariumLayout>> GetAllWithDetailsAsync()
    {
        return await _context.TerrariumLayouts
            .Include(l => l.User)
            .Include(l => l.Reviewer)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.Environment)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.Shapes)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.TankMethods)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.TerrariumImages)
            .Include(l => l.Terrarium)
                .ThenInclude(t => t.TerrariumAccessory)
                    .ThenInclude(ta => ta.Accessory)
            .OrderByDescending(l => l.CreatedDate)
            .ToListAsync();
    }
}