using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class BlogCategoryRepository : GenericRepository<BlogCategory>
    {
        public TerrariumGardenTechDBContext _context;
        public BlogCategoryRepository() { }

        public BlogCategoryRepository(TerrariumGardenTechDBContext context) => _context = context;

        public async Task<bool> AnyAsync(Expression<Func<BlogCategory, bool>> predicate)
        {
            return await _context.BlogCategories.AnyAsync(predicate);
        }

        // Nếu chưa có, thêm phương thức trả về IQueryable để linh hoạt hơn
        public IQueryable<BlogCategory> GetAll()
        {
            return _context.BlogCategories.AsQueryable();
        }


    }
}
