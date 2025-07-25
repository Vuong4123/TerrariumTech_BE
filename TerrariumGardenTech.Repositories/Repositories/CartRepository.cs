using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class CartRepository : GenericRepository<Cart>
    {
        public CartRepository(TerrariumGardenTechDBContext context) : base(context)
        {
        }

        // Cập nhật phương thức GetByUserIdAsync để bao gồm Accessory và TerrariumVariant
        public async Task<Cart> GetByUserIdAsync(int userId)
        {
            // Truy vấn giỏ hàng của người dùng theo userId
            return await _context.Carts
                .Include(c => c.CartItems)  // Bao gồm CartItems
                    .ThenInclude(ci => ci.Accessory)  // Bao gồm Accessory cho từng CartItem
                .Include(c => c.CartItems)  // Bao gồm CartItems
                    .ThenInclude(ci => ci.TerrariumVariant)  // Bao gồm TerrariumVariant cho từng CartItem
                .FirstOrDefaultAsync(c => c.UserId == userId);  // Lấy giỏ hàng đầu tiên có userId khớp
        }


        // Giữ nguyên phương thức ClearAsync
        public async Task<bool> ClearAsync(int userId)
        {
            var cart = await GetByUserIdAsync(userId);
            if (cart != null)
            {
                // Xóa các mục trong giỏ hàng
                _context.CartItems.RemoveRange(cart.CartItems); // Xóa hết CartItems
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }

}
