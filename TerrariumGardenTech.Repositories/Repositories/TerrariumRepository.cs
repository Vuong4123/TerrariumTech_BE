using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using static TerrariumGardenTech.Common.Enums.CommonData;

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
        queryable = queryable.Include(t => t.TerrariumImages);
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
    public async Task<Dictionary<int, int>> GetTerrariumPurchaseCountsAsync(List<int> terrariumIds)
    {
        // Tổng số lượng theo Terrarium, chỉ tính Order đã Paid
        var query =
            from oi in _context.OrderItems
            where oi.TerrariumVariantId != null
               && oi.Order != null
               && oi.Order.PaymentStatus == "Paid"
               && oi.TerrariumVariant != null
               && terrariumIds.Contains(oi.TerrariumVariant.TerrariumId)
            group oi by oi.TerrariumVariant.TerrariumId into g
            select new
            {
                TerrariumId = g.Key,
                Count = g.Sum(x =>
                    // ưu tiên quantity chuyên biệt cho variant, fallback về Quantity chung
                    (x.TerrariumVariantQuantity ?? x.Quantity ?? 0)
                )
            };

        var data = await query.ToListAsync();
        return data.ToDictionary(x => x.TerrariumId, x => x.Count);
    }


    public async Task<int> GetTerrariumPurchaseCountAsync(int terrariumId)
    {
        // Tổng số lượng đã mua của tất cả variants thuộc terrarium này
        var q =
            from oi in _context.OrderItems
            where oi.TerrariumVariantId != null
               && oi.TerrariumVariant != null
               && oi.TerrariumVariant.TerrariumId == terrariumId
               && oi.Order != null
               && oi.Order.PaymentStatus == "Paid"      // chỉ tính đơn đã thanh toán
            select (oi.TerrariumVariantQuantity ?? oi.Quantity ?? 0);

        return await q.SumAsync();
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

    // A) Top bán all-time (trả danh sách TerrariumId theo thứ tự giảm dần)
    public async Task<List<int>> GetBestSellerTerrariumIdsAsync(int topN)
    {
        var q =
            from oi in _context.OrderItems
            where oi.TerrariumVariantId != null
               && oi.Order != null
               && oi.Order.PaymentStatus == "Paid"
            group oi by oi.TerrariumVariant.TerrariumId into g
            orderby g.Sum(x => (x.TerrariumVariantQuantity ?? x.Quantity ?? 0)) descending
            select new { TerrariumId = g.Key };

        return await q.Take(topN).Select(x => x.TerrariumId).ToListAsync();
    }

    // B) Top bán theo khoảng ngày (vd 7d)
    public async Task<List<int>> GetBestSellerTerrariumIdsInRangeAsync(DateTime fromUtc, DateTime toUtc, int topN)
    {
        var q =
            from oi in _context.OrderItems
            where oi.TerrariumVariantId != null
               && oi.Order != null
               && oi.Order.PaymentStatus == "Paid"
               && oi.Order.OrderDate >= fromUtc
               && oi.Order.OrderDate <= toUtc
            group oi by oi.TerrariumVariant.TerrariumId into g
            orderby g.Sum(x => (x.TerrariumVariantQuantity ?? x.Quantity ?? 0)) descending
            select new { TerrariumId = g.Key };

        return await q.Take(topN).Select(x => x.TerrariumId).ToListAsync();
    }

    // C) Top rated (avg rating cao nhất) – có thể lọc minFeedback để loại spam
    public async Task<List<int>> GetTopRatedTerrariumIdsAsync(int topN)
    {
        if (topN <= 0) return new List<int>();

        var query = _context.Feedbacks
            .Where(fb => fb.Rating.HasValue
                         && fb.OrderItem != null
                         && fb.OrderItem.TerrariumVariant != null
                         && fb.OrderItem.TerrariumVariant.TerrariumId != null)
            .Select(fb => new
            {
                TerrariumId = (int)fb.OrderItem!.TerrariumVariant!.TerrariumId!, // ✅ ép kiểu
                Rating = (double)fb.Rating!.Value
            })
            .GroupBy(x => x.TerrariumId)
            .Select(g => new
            {
                TerrariumId = g.Key,
                AvgRating = g.Average(x => x.Rating)
            })
            .OrderByDescending(x => x.AvgRating)
            .ThenBy(x => x.TerrariumId) // tie-break cho kết quả ổn định
            .Take(topN)
            .Select(x => x.TerrariumId);

        return await query.ToListAsync();
    }

    // D) Newest
    public async Task<List<Terrarium>> GetNewestAsync(int topN)
    {
        return await _context.Terrariums
            .Include(t => t.TerrariumImages)
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.TerrariumId)
            .Take(topN)
            .ToListAsync();
    }

    // E) Batch: rating stats
    public async Task<Dictionary<int, (double AverageRating, int FeedbackCount)>> GetTerrariumRatingStatsAsync(List<int> terrariumIds)
    {
        var q =
            from fb in _context.Feedbacks
            where fb.OrderItem != null
               && fb.OrderItem.TerrariumVariantId != null
               && terrariumIds.Contains(fb.OrderItem.TerrariumVariant.TerrariumId)
               && fb.Rating.HasValue
            group fb by fb.OrderItem.TerrariumVariant.TerrariumId into g
            select new
            {
                TerrariumId = g.Key,
                Avg = g.Average(x => (double)x.Rating.Value),
                Cnt = g.Count()
            };

        var data = await q.ToListAsync();
        return data.ToDictionary(x => x.TerrariumId, x => (x.Avg, x.Cnt));
    }

    
    public async Task<List<Terrarium>> GetListByIdsAsync(List<int> terrariumIds)
    {
        return await _context.Terrariums
            .Include(t => t.TerrariumImages)
            .Where(t => terrariumIds.Contains(t.TerrariumId))
            .ToListAsync();
    }
    public Task<Terrarium?> GetByIdWithVariantsAsync(int terrariumId) =>
    _context.Terrariums
        .Include(t => t.TerrariumVariants)
        .Include(t => t.TerrariumImages)
        .SingleOrDefaultAsync(t => t.TerrariumId == terrariumId);
}