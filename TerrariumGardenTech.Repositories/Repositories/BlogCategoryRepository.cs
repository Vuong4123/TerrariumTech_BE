using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class BlogCategoryRepository : GenericRepository<BlogCategory>
{
    public TerrariumGardenTechDBContext _dbContext;
    //public BlogCategoryRepository() { }

    public BlogCategoryRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AnyAsync(Expression<Func<BlogCategory, bool>> predicate)
    {
        return await _context.BlogCategories.AnyAsync(predicate);
    }

    // Nếu chưa có, thêm phương thức trả về IQueryable để linh hoạt hơn
    public new IQueryable<BlogCategory> GetAll()
    {
        return _context.BlogCategories.AsQueryable();
    }
}