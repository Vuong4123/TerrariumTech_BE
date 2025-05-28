using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class CategoryRepository : GenericRepository<Category>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public CategoryRepository() { }
        public CategoryRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;

    }
}
