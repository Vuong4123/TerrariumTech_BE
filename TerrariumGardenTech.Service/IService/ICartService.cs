using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.RequestModel.Cart;
using TerrariumGardenTech.Service.RequestModel.Order;

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