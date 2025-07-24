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

}