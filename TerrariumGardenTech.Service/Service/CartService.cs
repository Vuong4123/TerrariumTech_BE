using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

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

        // Lấy giá của sản phẩm (ví dụ từ AccessoryId hoặc TerrariumVariantId)
        decimal unitPrice = await GetUnitPriceAsync(req.AccessoryId, req.TerrariumVariantId);

        var item = new CartItem
        {
            CartId = cart.CartId,
            AccessoryId = req.AccessoryId,
            TerrariumVariantId = req.TerrariumVariantId,
            Quantity = req.Quantity,
            UnitPrice = unitPrice, // Giá được tính từ cơ sở dữ liệu
            TotalPrice = req.Quantity * unitPrice, // Tính tổng giá trị cho món hàng
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CartItemRepository.CreateAsync(item);
        cart.CartItems.Add(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        return item;
    }

    private async Task<decimal> GetUnitPriceAsync(int? accessoryId, int? terrariumVariantId)
    {
        if (accessoryId.HasValue)
        {
            // Lấy giá của phụ kiện
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(accessoryId.Value);
            return accessory?.Price ?? 0; // Trả về giá phụ kiện
        }

        if (terrariumVariantId.HasValue)
        {
            // Lấy giá của terrarium variant
            var terrariumVariant = await _unitOfWork.TerrariumVariant.GetByIdAsync(terrariumVariantId.Value);
            return terrariumVariant?.Price ?? 0; // Trả về giá của terrarium variant
        }

        return 0; // Nếu không tìm thấy, trả về giá 0
    }



    public async Task<bool> UpdateItemAsync(int userId, int itemId, int quantity)
    {
        var cartItem = await _unitOfWork.CartItemRepository.GetByIdAsync(itemId);
        if (cartItem == null || cartItem.Cart.UserId != userId)
            return false;

        // Lấy lại giá sản phẩm từ AccessoryId hoặc TerrariumVariantId
        decimal unitPrice = await GetUnitPriceAsync(cartItem.AccessoryId, cartItem.TerrariumVariantId);

        // Cập nhật số lượng và tính lại TotalPrice
        cartItem.Quantity = quantity;
        cartItem.TotalPrice = quantity * unitPrice; // Tính lại giá trị tổng của món hàng
        cartItem.UpdatedAt = DateTime.UtcNow;

        // Cập nhật thông tin trong database
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