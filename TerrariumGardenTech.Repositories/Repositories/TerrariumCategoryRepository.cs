using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class TerrariumCategoryRepository : GenericRepository<TerrariumCategory>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        public TerrariumCategoryRepository() { }
        public TerrariumCategoryRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
        public async Task<bool> AnyAsync(Expression<Func<TerrariumCategory, bool>> predicate)
        {
            return await _context.TerrariumCategories.AnyAsync(predicate);
        }

        // Nếu chưa có, thêm phương thức trả về IQueryable để linh hoạt hơn
        public IQueryable<TerrariumCategory> GetAll()
        {
            return _context.TerrariumCategories.AsQueryable();
        }
    }
}
