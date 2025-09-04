using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using static TerrariumGardenTech.Common.Enums.CommonData;

public sealed class OrderRepository : GenericRepository<Order>
{
    public OrderRepository(TerrariumGardenTechDBContext context) : base(context)
    {
    }

    // ✅ FIXED - Add missing TerrariumVariant include
    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant) // ✅ THÊM
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory) // ✅ THÊM
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM
            .Include(o => o.Payment)
            .Include(o => o.User)
            .Include(o => o.Voucher)
              .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks)
                .Include(c => c.Refunds)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    

    // ✅ ALREADY GOOD - Has TerrariumVariant include
    public async Task<List<Order>> FindByUserAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM COMBO
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks) // ✅ THÊM FEEDBACKS
                .Include(c => c.Refunds)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate) // ✅ THÊM ORDER BY
            .ToListAsync(ct);
    }

    // ✅ ALREADY GOOD - Has TerrariumVariant include
    public async Task<Order> GetOrderbyIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM COMBO
            .Include(o => o.Payment)
              .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks)
            .Include(o => o.User) // ✅ THÊM USER INFO
            .Include(o => o.Address) // ✅ THÊM ADDRESS INFO
            .Include(o => o.Voucher) // ✅ THÊM VOUCHER INFO
            .Include(c => c.Refunds)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    // ✅ ALREADY GOOD - Has TerrariumVariant include
    public async Task<List<Order>> GetAllAsync2()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM COMBO
            .Include(o => o.Payment)
              .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks)
            .Include(o => o.User) // ✅ THÊM USER INFO
            .Include(o => o.Address) // ✅ THÊM ADDRESS INFO
            .Include(c => c.Refunds)
            .OrderByDescending(o => o.OrderDate) // ✅ THÊM ORDER BY
            .ToListAsync();
    }

    // ✅ ALREADY GOOD - Has TerrariumVariant include
    public async Task<List<Order>> GetAllWithStatus(string status)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM COMBO
            .Include(o => o.Payment)
              .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks)
            .Include(o => o.User) // ✅ THÊM USER INFO
            .Include(c => c.Refunds)
            .Where(o => o.Status.ToString() == status)
            .OrderByDescending(o => o.OrderDate) // ✅ THÊM ORDER BY
            .ToListAsync();
    }

    // ✅ ALREADY GOOD - Has TerrariumVariant include
    public async Task<Order?> GetByIdWithOrderItemsAsync(int id)
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM COMBO
            .Include(o => o.Payment)
              .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks)
                .Include(c => c.Refunds)
            .Where(o => o.OrderId == id)
            .SingleOrDefaultAsync();
    }

    // ✅ FIXED - Add missing includes
    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant) // ✅ THÊM
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory) // ✅ THÊM
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM
            .Include(o => o.Payment) // ✅ THÊM
              .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks)
                .Include(c => c.Refunds)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate) // ✅ THÊM ORDER BY
            .ToListAsync();
    }

    // ✅ ALREADY GOOD - Has TerrariumVariant include
    public async Task<(IEnumerable<Order> orders, int totalCount)> GetAllWithPaginationAsync(int page, int pageSize)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM COMBO
            .Include(o => o.User)
              .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks)
            .Include(o => o.Address)
            .Include(c => c.Refunds)
            .Include(o => o.Voucher) // ✅ THÊM VOUCHER
            .OrderByDescending(o => o.OrderDate);

        var totalCount = await query.CountAsync();

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    // ✅ ALREADY GOOD - Has TerrariumVariant include
    public async Task<(IEnumerable<Order> orders, int totalCount)> GetByUserWithPaginationAsync(int userId, int page, int pageSize)
    {
        var query = _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.TerrariumVariant)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Accessory)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Combo) // ✅ THÊM COMBO
            .Include(o => o.User)
            .Include(o => o.Address)
              .Include(o => o.OrderItems)
                .ThenInclude(f => f.Feedbacks)
                .Include(c => c.Refunds)
            .Include(o => o.Voucher) // ✅ THÊM VOUCHER
            .OrderByDescending(o => o.OrderDate);

        var totalCount = await query.CountAsync();

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }
    public async Task<decimal> GetTotalRevenueAsync()
    {
        var orders = await _context.Orders
        .Where(order => order.PaymentStatus == "Paid" &&
                        order.Status != OrderStatusData.Rejected &&
                        order.Status != OrderStatusData.Refunded &&
                        order.Status != OrderStatusData.Cancel)
        .ToListAsync();

        decimal total = orders.Sum(order => order.TotalAmount);

        // Ghi log tổng tiền của từng đơn hàng để kiểm tra
        //foreach (var order in orders)
        //{
        //    _logger.LogInformation($"Order ID: {order.OrderId}, TotalAmount: {order.TotalAmount}");
        //}

        return total;
    }
    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
