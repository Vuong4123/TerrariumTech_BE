using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
public class OrderRequestRefundRepository : GenericRepository<OrderRequestRefund>
    {
        public OrderRequestRefundRepository(TerrariumGardenTechDBContext dBContext) : base(dBContext)
        {
            
        }
        public async Task<List<OrderRequestRefund>> GetAllWithStatus(string status)
        {
            return await _context.OrderRequestRefunds
                .Where(r => r.Status == status)
                .ToListAsync();
        }
    }
}
