using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

/// <summary>Repository chuyên Order, kế thừa CRUD từ GenericRepository&lt;Order&gt;.</summary>
public sealed class OrderRepository : GenericRepository<Order>
{
    /// <summary>DI DbContext và truyền về lớp cha.</summary>
    public OrderRepository(TerrariumGardenTechDBContext context)
        : base(context)
    {
    }

    /// <summary>Lấy tất cả đơn của một user.</summary>
    public async Task<List<Order>> FindByUserAsync(
        int userId,
        CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<Order>GetOrderbyIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory)
            .Include(o => o.Payment)
            .Include(o => o.ReturnExchangeRequests)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<Order?> GetByIdWithOrderItemsAsync(int id)
    {
        return await _context.Set<Order>()
            .Include(m => m.OrderItems)
                .ThenInclude(m => m.TerrariumVariant)
            .Include(m => m.OrderItems)
                .ThenInclude(m => m.Accessory)
            .Where(m => m.OrderId == id).SingleOrDefaultAsync();
    }


    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Where(a => a.UserId == userId) // Bạn có thể thay thế "Contains" bằng cách tìm chính xác tên nếu cần
            .ToListAsync();
    }

}