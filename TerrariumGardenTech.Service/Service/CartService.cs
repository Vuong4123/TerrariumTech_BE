using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.ResponseModel.Cart;
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
        Console.WriteLine($"Attempting to fetch cart for user with ID {userId}");

        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId);

        // Kiểm tra nếu giỏ hàng không tồn tại
        if (cart == null)
        {
            Console.WriteLine($"No cart found for user with ID {userId}. Creating a new cart.");

            // Tạo giỏ hàng mới cho người dùng
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Lưu giỏ hàng mới vào cơ sở dữ liệu
            await _unitOfWork.CartRepository.CreateAsync(cart);
            Console.WriteLine($"Created new cart with ID {cart.CartId} for user with ID {userId}.");
        }
        else
        {
            // Logging khi tìm thấy giỏ hàng
            Console.WriteLine($"Found existing cart with ID {cart.CartId} for user with ID {userId}.");
        }

        return cart;
    }





    public async Task<Cart> GetCartByUserAsync(int userId)
    {
        return await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
    }

    public async Task<CartItemResponse> AddItemAsync(int userId, AddCartItemRequest req)
    {
        var cart = await GetOrCreateCartAsync(userId);
        decimal totalCartPrice = 0;

        // Tạo đối tượng CartItemResponse để trả về thông tin giỏ hàng đầy đủ
        var cartItemResponse = new CartItemResponse
        {
            CartId = cart.CartId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Xử lý khi có AccessoryId
        if (req.AccessoryId.HasValue && req.AccessoryQuantity.HasValue && req.AccessoryQuantity.Value > 0)
        {
            decimal unitPrice = await GetUnitPriceAsync(req.AccessoryId, null);  // Lấy giá của phụ kiện
            cartItemResponse.AccessoryId = req.AccessoryId;
            cartItemResponse.AccessoryQuantity = req.AccessoryQuantity.Value;  // Số lượng cho phụ kiện
            cartItemResponse.AccessoryUnitPrice = unitPrice;  // Giá của phụ kiện
            cartItemResponse.TotalPrice += req.AccessoryQuantity.Value * unitPrice;  // Tính tổng cho phụ kiện
        }

        // Xử lý khi có TerrariumVariantId
        if (req.TerrariumVariantId.HasValue && req.VariantQuantity.HasValue && req.VariantQuantity.Value > 0)
        {
            decimal unitPrice = await GetUnitPriceAsync(null, req.TerrariumVariantId);  // Lấy giá của variant
            cartItemResponse.TerrariumVariantId = req.TerrariumVariantId;
            cartItemResponse.TerrariumVariantQuantity = req.VariantQuantity.Value;  // Số lượng cho variant
            cartItemResponse.TerrariumVariantUnitPrice = unitPrice;  // Giá của variant
            cartItemResponse.TotalPrice += req.VariantQuantity.Value * unitPrice;  // Tính tổng cho variant
        }

        // Lưu vào cơ sở dữ liệu
        var cartItem = new CartItem
        {
            CartId = cartItemResponse.CartId,
            AccessoryId = cartItemResponse.AccessoryId,
            TerrariumVariantId = cartItemResponse.TerrariumVariantId,
            Quantity = cartItemResponse.Quantity,
            UnitPrice = cartItemResponse.UnitPrice,
            TotalPrice = cartItemResponse.TotalPrice,
            CreatedAt = cartItemResponse.CreatedAt,
            UpdatedAt = cartItemResponse.UpdatedAt
        };

        await _unitOfWork.CartItemRepository.CreateAsync(cartItem);
        await _unitOfWork.SaveAsync();

        return cartItemResponse;  // Trả về thông tin giỏ hàng đầy đủ
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




    public async Task<bool> UpdateItemAsync(int userId, int cartItemId, int? accessoryQuantity, int? variantQuantity)
    {
        // Lấy thông tin CartItem từ cơ sở dữ liệu
        var cartItem = await _unitOfWork.CartItemRepository.GetByIdAsync(cartItemId);

        if (cartItem == null)
        {
            Console.WriteLine($"CartItem with ID {cartItemId} not found.");
            return false;  // Không tìm thấy item
        }

        // Kiểm tra nếu Cart không tồn tại hoặc Cart không thuộc về user
        if (cartItem.Cart == null)
        {
            Console.WriteLine($"Cart with ID {cartItem.CartId} not found.");
            return false;  // Không tìm thấy cart
        }

        Console.WriteLine($"Checking ownership. CartUserId: {cartItem.Cart.UserId}, RequestUserId: {userId}");
        if (cartItem.Cart.UserId != userId)
        {
            Console.WriteLine($"User ID mismatch. CartUserId: {cartItem.Cart.UserId}, RequestUserId: {userId}");
            return false;  // Người dùng không sở hữu item này
        }

        decimal unitPrice = 0;

        // Cập nhật số lượng cho phụ kiện hoặc variant
        if (cartItem.AccessoryId.HasValue && accessoryQuantity.HasValue && accessoryQuantity.Value > 0)
        {
            unitPrice = await GetUnitPriceAsync(cartItem.AccessoryId, null);  // Lấy giá của phụ kiện
            cartItem.Quantity = accessoryQuantity.Value;
            cartItem.TotalPrice = cartItem.Quantity * unitPrice;
        }

        if (cartItem.TerrariumVariantId.HasValue && variantQuantity.HasValue && variantQuantity.Value > 0)
        {
            unitPrice = await GetUnitPriceAsync(null, cartItem.TerrariumVariantId);  // Lấy giá của variant
            cartItem.Quantity = variantQuantity.Value;
            cartItem.TotalPrice = cartItem.Quantity * unitPrice;
        }

        cartItem.UpdatedAt = DateTime.UtcNow;

        // Cập nhật vào cơ sở dữ liệu
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