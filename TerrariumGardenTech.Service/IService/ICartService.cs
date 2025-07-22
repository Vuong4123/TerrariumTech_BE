using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.IService;

public interface ICartService
{
    Task<Cart> GetOrCreateCartAsync(int userId);
    Task<Cart> GetCartByUserAsync(int userId);
    Task<CartItem> AddItemAsync(int userId, AddCartItemRequest req);
    Task<bool> UpdateItemAsync(int userId, int itemId, int quantity);
    Task<bool> RemoveItemAsync(int userId, int itemId);
    Task<bool> ClearCartAsync(int userId);
    Task<Order> CheckoutAsync(int userId, CheckoutRequest req);
}