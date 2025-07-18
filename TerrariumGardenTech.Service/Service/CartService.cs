using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Cart;
using TerrariumGardenTech.Service.RequestModel.Order;

namespace TerrariumGardenTech.Service.Service;

public class CartService : ICartService
{
    private readonly UnitOfWork _unitOfWork;

    public CartService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CartRepository.CreateAsync(cart);
        }

        return cart;
    }

    public async Task<Cart> GetCartByUserAsync(int userId)
    {
        return await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
    }

    public async Task<CartItem> AddItemAsync(int userId, AddCartItemRequest req)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var item = new CartItem
        {
            CartId = cart.CartId,
            AccessoryId = req.AccessoryId,
            TerrariumVariantId = req.TerrariumVariantId,
            Quantity = req.Quantity,
            UnitPrice = req.UnitPrice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CartItemRepository.CreateAsync(item);
        cart.CartItems.Add(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        return item;
    }

    public async Task<bool> UpdateItemAsync(int userId, int itemId, int quantity)
    {
        var cartItem = await _unitOfWork.CartItemRepository.GetByIdAsync(itemId);
        if (cartItem == null || cartItem.Cart.UserId != userId)
            return false;

        cartItem.Quantity = quantity;
        cartItem.TotalPrice = quantity * cartItem.UnitPrice;
        cartItem.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.CartItemRepository.UpdateAsync(cartItem);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> RemoveItemAsync(int userId, int itemId)
    {
        var cartItem = await _unitOfWork.CartItemRepository.GetByIdAsync(itemId);
        if (cartItem == null || cartItem.Cart.UserId != userId)
            return false;

        await _unitOfWork.CartItemRepository.RemoveAsync(cartItem);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
        if (cart == null)
            return false;

        await _unitOfWork.CartRepository.ClearAsync(userId);
        return true;
    }

    public async Task<Order> CheckoutAsync(int userId, CheckoutRequest req)
    {
        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
        if (cart == null || !cart.CartItems.Any())
            throw new InvalidOperationException("Giỏ hàng trống hoặc không tồn tại.");

        var order = new Order
        {
            UserId = userId,
            TotalAmount = cart.CartItems.Sum(item => item.TotalPrice),
            PaymentStatus = "Unpaid",
            ShippingStatus = "Unprocessed",
            Status = "Pending",
            OrderItems = cart.CartItems.Select(item => new OrderItem
            {
                AccessoryId = item.AccessoryId,
                TerrariumVariantId = item.TerrariumVariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        };

        await _unitOfWork.OrderRepository.CreateAsync(order);
        await _unitOfWork.CartRepository.ClearAsync(userId);

        return order;
    }
}