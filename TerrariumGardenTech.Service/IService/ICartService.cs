using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.ResponseModel.Cart;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.IService;

public interface ICartService
{
    Task<Cart> GetOrCreateCartAsync(int userId);
    Task<Cart> GetCartByUserAsync(int userId);
    Task<CartItemResponse> AddItemAsync(int userId, AddCartItemRequest req);

    // Cập nhật phương thức UpdateItemAsync để hỗ trợ 4 tham số
    Task<bool> UpdateItemAsync(int userId, int itemId, int? accessoryQuantity, int? variantQuantity);

    Task<bool> RemoveItemAsync(int userId, int itemId);
    Task<bool> ClearCartAsync(int userId);
    Task<Order> CheckoutAsync(int userId, CheckoutRequest req);
}
