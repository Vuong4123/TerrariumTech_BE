using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class CartItemRepository : GenericRepository<CartItem>
{
    public CartItemRepository(TerrariumGardenTechDBContext context) : base(context)
    {
    }
}