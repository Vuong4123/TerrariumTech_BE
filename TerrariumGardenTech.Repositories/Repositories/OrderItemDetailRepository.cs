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
    public class OrderItemDetailRepository : GenericRepository<OrderItemDetail>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;

        public OrderItemDetailRepository(TerrariumGardenTechDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // Lấy tất cả chi tiết của một OrderItem
        public async Task<List<OrderItemDetail>> GetOrderItemDetailsByOrderItemIdAsync(int orderItemId)
        {
            return await _dbContext.OrderItemDetails
                .Where(o => o.OrderItemId == orderItemId)
                .ToListAsync();
        }
    }
}
