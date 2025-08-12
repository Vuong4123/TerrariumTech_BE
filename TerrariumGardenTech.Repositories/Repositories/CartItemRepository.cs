using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class CartItemRepository : GenericRepository<CartItem>
{
    public CartItemRepository(TerrariumGardenTechDBContext context) : base(context)
    {
    }

    public async Task<CartItem?> GetByIdAsync(int cartItemId)
    {
        return await _context.CartItems
            .Include(ci => ci.Accessory)  // Bao gồm Accessory
            .Include(ci => ci.TerrariumVariant)  // Bao gồm TerrariumVariant
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
    }
    public async Task<CartItem> GetByIdWithCartAsync(int itemId)
    {
        return await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.CartItemId == itemId);
    }

    // Kiểm tra sản phẩm có trong giỏ hàng chưa (chỉ có Accessory hoặc chỉ có Terrarium)
    public async Task<CartItem?> GetExistingItemAsync(int cartId, int? accessoryId, int? terrariumVariantId)
    {


        // Không nhận case null/null
        if (accessoryId == null && terrariumVariantId == null)
            return null;  // <-- trả về CartItem? (không Task)

        return await _context.CartItems.FirstOrDefaultAsync(ci =>
            ci.CartId == cartId &&
            ci.AccessoryId == accessoryId &&
            ci.TerrariumVariantId == terrariumVariantId);
    }

}