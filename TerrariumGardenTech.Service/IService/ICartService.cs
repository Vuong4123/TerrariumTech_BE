using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.ResponseModel.Cart;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface ICartService
{
    Task<IBusinessResult> GetCartAsync(int userId);
    Task<Cart> GetOrCreateCartAsync(int userId);
    Task<Cart> GetCartByUserAsync(int userId);
    Task<CartBundleResponse> AddItemAsync(int userId, AddCartItemRequest request);

    // Cập nhật phương thức UpdateItemAsync để hỗ trợ 4 tham số
    Task<IBusinessResult> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);

    Task<bool> RemoveItemAsync(int userId, int itemId);
    Task<bool> ClearCartAsync(int userId);
    Task<IBusinessResult> CheckoutAsync(int userId);
}
