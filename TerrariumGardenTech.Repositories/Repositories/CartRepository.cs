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
    public class CartRepository : GenericRepository<Cart>
    {
        public CartRepository(TerrariumGardenTechDBContext context) : base(context) { }

        public async Task<Cart> GetByUserIdAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> ClearAsync(int userId)
        {
            var cart = await GetByUserIdAsync(userId);
            if (cart != null)
            {
                // Xóa các mục trong giỏ hàng
                _context.CartItems.RemoveRange(cart.CartItems);  // Xóa hết CartItems
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

    }


}
