using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TerrariumRepository : GenericRepository<Terrarium>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public TerrariumRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Terrarium>> FilterTerrariumsAsync(int? environmentId, int? shapeId, int? tankMethodId)
    {
        var query = _context.Terrariums.AsQueryable();

        if (environmentId.HasValue) query = query.Where(t => t.EnvironmentId == environmentId.Value);

        if (shapeId.HasValue) query = query.Where(t => t.ShapeId == shapeId.Value);

        if (tankMethodId.HasValue) query = query.Where(t => t.TankMethodId == tankMethodId.Value);

        return await query
                .Include(t => t.TerrariumImages)  // Nạp dữ liệu TerrariumImages
                .ToListAsync();
    }

    public async Task<IEnumerable<Terrarium>> GetAllByTankMethodIdAsync(int tankMethodId)
    {
        return await _context.Terrariums
            .Where(t => t.TankMethodId == tankMethodId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Terrarium>> GetAllByShapeIdAsync(int shapeId)
    {
        return await _context.Terrariums
            .Where(t => t.ShapeId == shapeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Terrarium>> GetAllByEnvironmentIdAsync(int environmentId)
    {
        return await _context.Terrariums
            .Where(t => t.EnvironmentId == environmentId)
            .ToListAsync();
    }
    // Nạp dữ liệu Terrarium cùng với ảnh (TerrariumImages)
    public async Task<(IEnumerable<Terrarium>, int)> GetFilterAndPagedAsync(TerrariumGetAllRequest request)
    {
        var queryable = _dbContext.Terrariums.AsQueryable(); 
        queryable = Include(queryable, request.IncludeProperties);
        var totalOrigin = queryable.Count();

        // filter
        
        // end
        
        queryable = request.Pagination.IsPagingEnabled ? GetQueryablePagination(queryable, request) : queryable;

        return (await queryable.ToListAsync(), totalOrigin);
    }
    // Lấy dữ liệu Terrarium theo ID kèm theo hình ảnh
    public async Task<Terrarium> GetTerrariumWithImagesByIdAsync(int id)
    {
        // Sử dụng Include để nạp dữ liệu TerrariumImages liên quan
        return await _dbContext.Terrariums
            .Include(t => t.TerrariumImages) // Nạp dữ liệu TerrariumImages
            .Include(t => t.TerrariumAccessory)  // Nạp dữ liệu Accessory liên quan
                .ThenInclude(ta => ta.Accessory)  // Nạp dữ liệu Accessory
            .FirstOrDefaultAsync(t => t.TerrariumId == id); // Tìm theo ID// Lọc theo TerrariumId
    }

    // Lấy danh sách Terrarium theo danh sách ID để delete theo accessory
    public async Task<List<Terrarium>> GetTerrariumByIdsAsync(List<int> terrariumIds)
    {
        return await _dbContext
            .Terrariums
            .Where(t => terrariumIds.Contains(t.TerrariumId)).ToListAsync();
    }

    // Sửa lại GetAllAsync() trả về IQueryable<T>
    public async Task<List<Terrarium>> GetAllAsyncV2()
    {
        return _dbContext.Terrariums.ToList(); // Trả về IQueryable
    }

    //public async Task<IEnumerable<Terrarium>> GetByNameAsync(string name)
    //{
    //    return await _dbContext.Terrariums
    //        .Where(a => a.TerrariumName.Contains(name)) // Bạn có thể thay thế "Contains" bằng cách tìm chính xác tên nếu cần
    //        .Include(t => t.TerrariumAccessory)  // Nạp dữ liệu Accessory liên quan
    //            .ThenInclude(ta => ta.Accessory)  // Nạp dữ liệu Accessory
    //        .Include(t => t.TerrariumImages).ToListAsync();
    //}


    public async Task<Dictionary<int, (double AverageRating, int FeedbackCount)>> GetTerrariumRatingStatsAsync(IEnumerable<int> terrariumIds)
    {
        var query = from v in _context.TerrariumVariants
                    join oi in _context.OrderItems on v.TerrariumVariantId equals oi.TerrariumVariantId
                    join fb in _context.Feedbacks on oi.OrderItemId equals fb.OrderItemId
                    where terrariumIds.Contains(v.TerrariumId)
                          && fb.Rating != null
                          && !fb.IsDeleted
                    group fb by v.TerrariumId into g
                    select new
                    {
                        TerrariumId = g.Key,
                        AverageRating = g.Average(x => x.Rating.Value),
                        FeedbackCount = g.Count()
                    };

        var result = await query.ToListAsync();

        return result.ToDictionary(
            x => x.TerrariumId,
            x => (x.AverageRating, x.FeedbackCount)
        );
    }

    public async Task<List<Terrarium>> GetTerrariumByNameAsync(string terrariumName)
    {
        return await _context.Terrariums
            .Include(t => t.TerrariumImages)
            .Include(t => t.TerrariumAccessory)
                .ThenInclude(ta => ta.Accessory)
            .Where(t => t.TerrariumName.Contains(terrariumName))
            .ToListAsync();
    }
}