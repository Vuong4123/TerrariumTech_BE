using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface ICartService
    {
        // Basic cart operations
        Task<IBusinessResult> GetCartAsync(int userId);
        Task<Cart> GetOrCreateCartAsync(int userId);
        Task<Cart> GetCartByUserAsync(int userId);
        Task<IBusinessResult> ClearCartAsync(int userId);

        // Single items and bundle operations
        Task<IBusinessResult> AddItemAsync(int userId, AddCartItemRequest request);
        Task<IBusinessResult> AddMultipleItemAsync(int userId, AddCartItemMultipleRequest request);
        Task<IBusinessResult> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
        Task<IBusinessResult> RemoveFromCartAsync(int userId, int cartId);

        // Combo operations
        Task<IBusinessResult> AddComboToCartAsync(int userId, AddComboToCartRequest request);
        Task<IBusinessResult> UpdateComboQuantityAsync(int userId, int cartId, int quantity);

        // Checkout
        Task<IBusinessResult> CheckoutAsync(int userId);

        // Utility methods
        Task<IBusinessResult> GetCartSummaryAsync(int userId);
        Task<IBusinessResult> ValidateCartAsync(int userId);

    }
}
