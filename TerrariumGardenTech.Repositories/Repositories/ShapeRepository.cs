namespace TerrariumGardenTech.Repositories.Repositories
{
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using TerrariumGardenTech.Repositories.Base;
    using TerrariumGardenTech.Repositories.Entity;

    public class ShapeRepository : GenericRepository<Shape>
    {
        public TerrariumGardenTechDBContext _dbContext;

        public ShapeRepository() { }

        public ShapeRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
        
        public async Task<Shape?> GetByName(string? name)
        {
            return await _dbContext.Set<Shape>().FirstOrDefaultAsync(s => s.ShapeName == name);
        }
    }
}