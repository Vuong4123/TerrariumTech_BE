using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class ShapeRepository : GenericRepository<Shape>
{
    public TerrariumGardenTechDBContext _dbContext;

    //public ShapeRepository() { }

    public ShapeRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Shape?> GetByName(string? name)
    {
        return await _dbContext.Set<Shape>().FirstOrDefaultAsync(s => s.ShapeName == name);
    }
}