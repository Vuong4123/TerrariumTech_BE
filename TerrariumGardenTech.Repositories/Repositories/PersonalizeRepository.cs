using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class PersonalizeRepository : GenericRepository<Personalize>
{
    private readonly TerrariumGardenTechDBContext _dbContexxt;

    public PersonalizeRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContexxt = dbContext;
    }
    public async Task<Personalize> GetByUserIdAsync(int userId)
    {
        return await _context.Personalizes
            .Where(p => p.UserId == userId)
            .FirstOrDefaultAsync();
    }
    public async Task<List<Personalize>> GetByUserId(int userId)
    {
        return await _context.Personalizes
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }
}