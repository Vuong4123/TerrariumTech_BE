using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public sealed class OrderItemRepository : GenericRepository<OrderItem>
    {
        public OrderItemRepository(TerrariumGardenTechDBContext ctx)
            : base(ctx) { }

        public async Task<List<OrderItem>> FindByOrderIdAsync(int orderId) =>
            await _context.Set<OrderItem>()
                          .Where(x => x.OrderId == orderId)
                          .ToListAsync();
    }
}
