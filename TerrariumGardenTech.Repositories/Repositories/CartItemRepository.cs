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

    //public Task<CartItem?> GetByIdAsync(int id) =>
    //   _context.CartItems.FindAsync(id).AsTask();

    //public Task<CartItem?> GetByIdWithCartAsync(int id) =>
    //    _context.CartItems.Include(x => x.Cart)
    //                  .SingleOrDefaultAsync(x => x.CartItemId == id);

    //public Task<CartItem?> GetExistingItemAsync(int cartId, int? accessoryId, int? variantId) =>
    //    _context.CartItems.SingleOrDefaultAsync(ci =>
    //        ci.CartId == cartId &&
    //        ci.AccessoryId == accessoryId &&
    //        ci.TerrariumVariantId == variantId);

    // ✅ dùng để trả về 1 line đầy đủ sau PATCH
    public Task<CartItem?> GetHydratedByIdAsync(int id) =>
        _context.CartItems
            .Include(ci => ci.Accessory)
            .Include(ci => ci.TerrariumVariant)
                .ThenInclude(v => v.Terrarium)
                    .ThenInclude(t => t.TerrariumVariants)
            .Include(ci => ci.Cart)
            .SingleOrDefaultAsync(ci => ci.CartItemId == id);

}