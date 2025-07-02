using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class BlogRepository : GenericRepository<Blog>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        //public BlogRepository()
        //{
        //}
        public BlogRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
    }
}
