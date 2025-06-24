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
    public class CategoryRepository : GenericRepository<Category>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;
        //public CategoryRepository() { }
        public CategoryRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
        public async Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate)
        {
            return await _context.Categories.AnyAsync(predicate);
        }

        // Nếu chưa có, thêm phương thức trả về IQueryable để linh hoạt hơn
        public IQueryable<Category> GetAll()
        {
            return _context.Categories.AsQueryable();
        }

    }
}
