using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using static TerrariumGardenTech.Common.Enums.CommonData;

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
    //Thêm parameter phân biệt bundle
    public async Task<CartItem?> GetExistingTerrariumItemAsync(int cartId, int? variantId, bool isBundle = false)
    {
        var query = _context.CartItems
            .Where(ci => ci.CartId == cartId &&
                        ci.TerrariumVariantId == variantId &&
                        ci.ItemType == CartItemType.MAIN_ITEM);

        if (isBundle)
        {
            // Bundle: có accessories đi kèm (có children)
            query = query.Where(ci => _context.CartItems.Any(child => child.ParentCartItemId == ci.CartItemId));
        }
        else
        {
            // Đơn lẻ: không có accessories đi kèm (không có children)
            query = query.Where(ci => !_context.CartItems.Any(child => child.ParentCartItemId == ci.CartItemId));
        }

        return await query.FirstOrDefaultAsync();
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
    // Method cho terrarium
    public async Task<CartItem?> GetExistingTerrariumItemAsync(
        int cartId,
        int? terrariumVariantId)
    {
        return await _context.CartItems.FirstOrDefaultAsync(ci =>
            ci.CartId == cartId &&
            ci.TerrariumVariantId == terrariumVariantId &&
            ci.ItemType == CartItemType.MAIN_ITEM);
    }

    // ✅ Riêng cho single accessory
    public async Task<CartItem?> GetExistingSingleAccessoryItemAsync(int cartId, int accessoryId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId &&
                                      ci.AccessoryId == accessoryId &&
                                      ci.ItemType == CartItemType.SINGLE);
    }

    // ✅ Cho accessory trong bundle/main item
    public async Task<CartItem?> GetExistingAccessoryInBundleAsync(int cartId, int accessoryId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId &&
                                      ci.AccessoryId == accessoryId &&
                                      ci.ItemType == CartItemType.MAIN_ITEM);
    }
    public async Task<CartItem> GetExistingItemMAIN_ITEMAsync(int cartId, int terrariumId, int? variantId)
    {
        return await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci =>
                ci.CartId == cartId &&
                ci.TerrariumId == terrariumId &&
                ci.TerrariumVariantId == variantId &&
                ci.ItemType == CartItemType.MAIN_ITEM);
    }

    // Kiểm tra sản phẩm có trong giỏ hàng chưa (chỉ có Accessory hoặc chỉ có Terrarium)
    public async Task<CartItem?> GetExistingCartItemAsync(int cartId, int? terrariumVariantId)
    {
        // Không nhận case null/null
        if (terrariumVariantId == null)
            return null;  // <-- trả về CartItem? (không Task)

        return await _context.CartItems.FirstOrDefaultAsync(ci =>
            ci.CartId == cartId &&
            ci.TerrariumVariantId == terrariumVariantId);
    }
    // Kiểm tra sản phẩm có trong giỏ hàng chưa (chỉ có Accessory hoặc chỉ có Terrarium)
    public async Task<CartItem?> GetExistingTerrariumItemAsync(int cartId, int TerrariumVarientId)
    {
        return await _context.CartItems.FirstOrDefaultAsync(ci =>
            ci.CartId == cartId &&
            ci.TerrariumVariantId == TerrariumVarientId);
    }
    public async Task<List<CartItem>> GetBundleAccessoriesByParentIdAsync(int parentCartItemId)
    {
        return await _context.CartItems
            .Include(ci => ci.Accessory)
            .Where(ci => ci.ParentCartItemId == parentCartItemId &&
                       ci.ItemType == CartItemType.BUNDLE_ACCESSORY)
            .ToListAsync();
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