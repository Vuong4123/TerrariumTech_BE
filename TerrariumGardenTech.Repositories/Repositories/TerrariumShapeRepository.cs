namespace TerrariumGardenTech.Repositories.Repositories
{
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using TerrariumGardenTech.Repositories.Base;
    using TerrariumGardenTech.Repositories.Entity;

    public class TerrariumShapeRepository : GenericRepository<TerrariumShape>
    {
        public TerrariumGardenTechDBContext _dbContext;

        //public TerrariumShapeRepository() { }

        public TerrariumShapeRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;

        // public async Task<Shape?> GetByName(string? name)
        // {
        //     return await _dbContext.Set<Shape>().FirstOrDefaultAsync(s => s.ShapeName == name);
        // }
        public async Task<List<TerrariumShape>> GetAllTerrariumByShapes(int shapeId)
        {
            return await _dbContext
                .TerrariumShapes
                .AsNoTracking()
                .Include(te => te.Terrarium)
                .Where(te => te.ShapeId == shapeId)
                .ToListAsync();
        }
        public async Task<List<TerrariumShape>> GetTerrariumShapesByTerrariumIdAsync(int terrariumId)
        {
            return await _dbContext.TerrariumShapes
                          .Where(ts => ts.TerrariumId == terrariumId)
                          .ToListAsync();
        }
    }
}