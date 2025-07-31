using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TerrariumGardenTech.Common.RequestModel.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Base;

public class GenericRepository<T> where T : class
{
    protected TerrariumGardenTechDBContext _context;

    /* Constructor DI  ➜  dùng AddDbContext trong Program.cs */
    public GenericRepository(TerrariumGardenTechDBContext context)
    {
        _context = context;
    }

    // Chỉ giữ hàm tạo có tham số
    public GenericRepository()
    {
        _context ??= new TerrariumGardenTechDBContext();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public TerrariumGardenTechDBContext Context()
    {
        return _context;
    }

    public List<T> GetAll()
    {
        return _context.Set<T>().ToList();
    }

    public IQueryable<T> GetAllV2()
    {
        return _context.Set<T>();
    }
    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public void Create(T entity)
    {
        _context.Add(entity);
        _context.SaveChanges();
    }

    public async Task<int> CreateAsync(T entity)
    {
        _context.Add(entity);
        return await _context.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        var tracker = _context.Attach(entity);
        tracker.State = EntityState.Modified;
        _context.SaveChanges();
    }

    public async Task<int> UpdateAsync(T entity)
    {
        var tracker = _context.Attach(entity);
        tracker.State = EntityState.Modified;
        return await _context.SaveChangesAsync();
    }

    public bool Remove(T entity)
    {
        _context.Remove(entity);
        _context.SaveChanges();
        return true;
    }

    public async Task<bool> RemoveAsync(T entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public T? GetById(int id)
    {
        return _context.Set<T>().Find(id);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public T? GetById(string code)
    {
        return _context.Set<T>().Find(code);
    }

    public async Task<T?> GetByIdAsync(string code)
    {
        return await _context.Set<T>().FindAsync(code);
    }

    public T? GetById(Guid code)
    {
        return _context.Set<T>().Find(code);
    }

    public async Task<T?> GetByIdAsync(Guid code)
    {
        return await _context.Set<T>().FindAsync(code);
    }

    // Hàm SaveChangesAsync
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> expression, bool hasTrackings = true)
    {
        return hasTrackings
            ? await _context.Set<T>().FirstOrDefaultAsync(expression)
            : await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(expression);
    }

    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<TerrariumImage> images)
    {
        _context.TerrariumImages.RemoveRange(images);
        await _context.SaveChangesAsync();
    }
    
    // Add more pagination and include
    
    public static IQueryable<T> Include(IQueryable<T> queryable, string[]? includeProperties)
    {
        if (includeProperties != null)
            foreach (var property in includeProperties)
                queryable = queryable.Include(property);

        return queryable;
    }
    
    public IQueryable<T> GetQueryablePagination(IQueryable<T> queryable, GetQueryableQuery query)
    {
        queryable = queryable
            .Skip((query.Pagination.PageNumber - 1) * query.Pagination.PageSize)
            .Take(query.Pagination.PageSize);
        return queryable;
    }
    public IQueryable<T> Include(Expression<Func<T, object>> includeProperties)
    {
        return _context.Set<T>().Include(includeProperties);
    }
    public IQueryable<T> Include(params string[] includeProperties)
    {
        var query = _context.Set<T>().AsQueryable();
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }
        return query;
    }
}