using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Accessory;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class AccessoryRepository : GenericRepository<Accessory>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    //public AccessoryRepository() { }
    public AccessoryRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    // Add any specific methods for AccessoryRepository here

    public async Task<List<Accessory?>> GetByName(List<string?> name)
    {
        return await _dbContext.Set<Accessory>()
            .Where(e => name.Contains(e.Name)) // Tìm tất cả các Accessory có tên nằm trong danh sách names
            .ToListAsync();
    }
    public Task<Accessory?> GetByIdAsync(int id)
        => _dbContext.Accessories.FirstOrDefaultAsync(a => a.AccessoryId == id);

    public Task<Accessory?> GetByIdWithImagesAsync(int id) // <-- mới
        => _dbContext.Accessories
               .Include(a => a.AccessoryImages)
               .FirstOrDefaultAsync(a => a.AccessoryId == id);
    public async Task<IEnumerable<Accessory>> FilterAccessoryAsync(int? categoryId)
    {
        return await _context.Accessories.Where(a => a.CategoryId == categoryId).ToListAsync();
    }

    // Nạp dữ liệu Accessory cùng với ảnh (AccessoryImages)
    public async Task<(IEnumerable<Accessory>, int)> GetFilterAndPagedAsync(AccessoryGetAllRequest request)
    {
        var queryable = _dbContext.Accessories.AsQueryable();
        queryable = Include(queryable, request.IncludeProperties);
        var totalOrigin = queryable.Count();

        // filter

        // end

        queryable = request.Pagination.IsPagingEnabled ? GetQueryablePagination(queryable, request) : queryable;

        return (await queryable.ToListAsync(), totalOrigin);
    }
    // Lấy dữ liệu Terrarium theo ID kèm theo hình ảnh
    public async Task<Accessory> GetAccessoryWithImagesByIdAsync(int id)
    {
        // Sử dụng Include để nạp dữ liệu TerrariumImages liên quan
        return await _dbContext.Accessories
            .Include(t => t.AccessoryImages) // Nạp dữ liệu TerrariumImages
            .FirstOrDefaultAsync(t => t.AccessoryId == id); // Tìm theo ID// Lọc theo TerrariumId
    }
    // Tìm kiếm Accessory theo tên
    public async Task<IEnumerable<Accessory>> GetByNameAsync(string name)
    {
        return await _dbContext.Accessories
            .Where(a => a.Name.Contains(name)) // Bạn có thể thay thế "Contains" bằng cách tìm chính xác tên nếu cần
            .Include(t => t.AccessoryImages)
            .ToListAsync();
    }

    public async Task<Dictionary<int, (double AverageRating, int FeedbackCount)>> GetAccessoryRatingStatsAsync(IEnumerable<int> accessoryIds)
    {
        var query = from oi in _context.OrderItems
                    join fb in _context.Feedbacks on oi.OrderItemId equals fb.OrderItemId
                    where oi.AccessoryId != null
                      && accessoryIds.Contains(oi.AccessoryId.Value)   // Dùng .Value nếu AccessoryId là int?
                      && fb.Rating != null
                      && !fb.IsDeleted
                    group fb by oi.AccessoryId into g
                    select new
                    {
                        AccessoryId = g.Key.Value,  // .Value nếu group theo int?
                        AverageRating = g.Average(x => x.Rating.Value),
                        FeedbackCount = g.Count()
                    };

        var result = await query.ToListAsync();

        return result.ToDictionary(
            x => x.AccessoryId,
            x => (x.AverageRating, x.FeedbackCount)
        );
    }

    public async Task<Dictionary<int, int>> GetAccessoryPurchaseCountsAsync(IEnumerable<int> accessoryIds)
    {
        var idList = accessoryIds.ToList(); // đảm bảo List<int> để dùng Contains ổn định

        var dict = await _context.OrderItems
            .Where(oi => oi.AccessoryId.HasValue && idList.Contains(oi.AccessoryId.Value))
            .Where(oi => oi.Order.Status == OrderStatusEnum.Completed) // chỉ tính đơn đã hoàn tất
            .GroupBy(oi => oi.AccessoryId!.Value)
            .Select(g => new
            {
                AccessoryId = g.Key,
                // Ưu tiên AccessoryQuantity, nếu null thì dùng Quantity, nếu null nữa thì 0
                Total = g.Sum(x => (x.AccessoryQuantity ?? x.Quantity ?? 0))
            })
            .ToDictionaryAsync(x => x.AccessoryId, x => x.Total);

        return dict;
    }
}