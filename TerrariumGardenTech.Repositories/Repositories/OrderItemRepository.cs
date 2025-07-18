using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public sealed class OrderItemRepository : GenericRepository<OrderItem>
{
    public OrderItemRepository(TerrariumGardenTechDBContext ctx)
        : base(ctx)
    {
    }

    public async Task<List<OrderItem>> FindByOrderIdAsync(int orderId)
    {
        return await _context.Set<OrderItem>()
            .Where(x => x.OrderId == orderId)
            .ToListAsync();
    }
}