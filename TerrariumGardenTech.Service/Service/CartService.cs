using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;

using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.ResponseModel.Cart;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Common.ResponseModel.OrderItem;

using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

using TerrariumGardenTech.Common.Enums;


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


    public async Task<IBusinessResult> GetCartAsync(int userId)
    {
        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
        if (cart == null)
            return new BusinessResult (Const.FAIL_READ_CODE,"không có giỏ hàng");
        // Lấy thông tin người dùng từ bảng User (giả sử bạn có repository User)
        var user = await _unitOfWork.User.GetByIdAsync(userId);
        if (user == null)
            return new BusinessResult(Const.FAIL_READ_CODE, $"User with ID {userId} not found.");
        var cartResponse = new CartResponse
        {
            CartId = cart.CartId,
            UserId = userId,
            User = user.Email, // Hoặc lấy từ DB nếu có
            CartItems = new List<CartItemResponse>(),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };

        decimal totalCartPrice = 0;
        int totalCartQuantity = 0;

        foreach (var cartItem in cart.CartItems)
        {
            var cartItemResponse = new CartItemResponse
            {
                CartItemId = cartItem.CartItemId,
                CartId = cartItem.CartId,
                AccessoryId = cartItem.AccessoryId,
                TerrariumVariantId = cartItem.TerrariumVariantId,
                Item = new List<CartItemDetail>(),
                CreatedAt = cartItem.CreatedAt,
                UpdatedAt = cartItem.UpdatedAt
            };

            decimal cartItemTotalPrice = 0;
            int cartItemQuantity = 0;

            // Accessory
            if (cartItem.AccessoryId.HasValue && cartItem.AccessoryQuantity.HasValue && cartItem.AccessoryQuantity.Value > 0)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                if (accessory != null)
                {
                    var quantity = cartItem.AccessoryQuantity.Value;
                    var price = accessory.Price;

                    var accessoryDetail = new CartItemDetail
                    {
                        ProductName = accessory.Name,
                        Quantity = quantity,
                        Price = price,
                        TotalPrice = price * quantity
                    };

                    cartItemResponse.Item.Add(accessoryDetail);
                    cartItemTotalPrice += accessoryDetail.TotalPrice;
                    cartItemQuantity += quantity;
                }
            }

            // Terrarium
            if (cartItem.TerrariumVariantId.HasValue && cartItem.TerrariumVariantQuantity.HasValue && cartItem.TerrariumVariantQuantity.Value > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var quantity = cartItem.TerrariumVariantQuantity.Value;
                    var price = variant.Price;

                    var terrariumDetail = new CartItemDetail
                    {
                        ProductName = $"Terrarium {variant.VariantName}",
                        Quantity = quantity,
                        Price = price,
                        TotalPrice = price * quantity
                    };

                    cartItemResponse.Item.Add(terrariumDetail);
                    cartItemTotalPrice += terrariumDetail.TotalPrice;
                    cartItemQuantity += quantity;
                }
            }

            cartItemResponse.TotalCartPrice = cartItemTotalPrice;
            cartItemResponse.TotalCartQuantity = cartItemQuantity;

            cartResponse.CartItems.Add(cartItemResponse);
            totalCartPrice += cartItemTotalPrice;
            totalCartQuantity += cartItemQuantity;
        }

        cartResponse.TotalCartPrice = totalCartPrice;
        cartResponse.TotalCartQuantity = totalCartQuantity;

        return new BusinessResult(Const.SUCCESS_READ_CODE,Const.SUCCESS_READ_MSG, cartResponse);
    }


    public async Task<Cart> GetCartByUserAsync(int userId)
    {
        return await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
    }

    public async Task<CartItemResponse> AddItemAsync(int userId, AddCartItemRequest request)
    {
        // Lấy giỏ hàng của người dùng, nếu không có thì tạo mới
        var cart = await GetOrCreateCartAsync(userId);
        // Kiểm tra sản phẩm đã tồn tại trong giỏ hàng thông qua repository
        var existingItem = await _unitOfWork.CartItemRepository.GetExistingItemAsync(
            cart.CartId,
            request.AccessoryId,
            request.TerrariumVariantId
        );

        DateTime now = DateTime.UtcNow;

        CartItem cartItem;
        if (existingItem != null)
        {
            // Nếu sản phẩm đã tồn tại trong giỏ hàng
            // Cộng dồn số lượng cho Accessory nếu có
            if (request.AccessoryQuantity.HasValue && request.AccessoryQuantity.Value > 0)
            {
                existingItem.AccessoryQuantity += request.AccessoryQuantity.Value;
            }

            // Cộng dồn số lượng cho Terrarium nếu có
            if (request.VariantQuantity.HasValue && request.VariantQuantity.Value > 0)
            {
                existingItem.TerrariumVariantQuantity += request.VariantQuantity.Value;
            }

            // Cập nhật lại số lượng tổng
            existingItem.Quantity = (existingItem.AccessoryQuantity ?? 0) + (existingItem.TerrariumVariantQuantity ?? 0);

            decimal totalPrice = 0;
            // Tính lại giá trị tổng và đơn giá
            if (existingItem.AccessoryId.HasValue)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(existingItem.AccessoryId.Value);
                var price = await GetUnitPriceAsync(existingItem.AccessoryId, null);
                totalPrice += price * (existingItem.AccessoryQuantity ?? 0);
            }

            if (existingItem.TerrariumVariantId.HasValue)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(existingItem.TerrariumVariantId.Value);
                var price = await GetUnitPriceAsync(null, existingItem.TerrariumVariantId);
                totalPrice += price * (existingItem.TerrariumVariantQuantity ?? 0);
            }

            // Cập nhật lại tổng giá và đơn giá
            existingItem.TotalPrice = totalPrice;
            existingItem.UnitPrice = existingItem.Quantity > 0 ? totalPrice / existingItem.Quantity : 0;
            existingItem.UpdatedAt = now;

            await _unitOfWork.CartItemRepository.UpdateAsync(existingItem);
            cartItem = existingItem;
        }
        else
        {
            // Nếu chưa có sản phẩm trong giỏ hàng thì tạo mới
            decimal totalPrice = 0;
            int totalQuantity = 0;

            if (request.AccessoryId.HasValue)
            {
                var accessoryPrice = await GetUnitPriceAsync(request.AccessoryId, null);
                totalPrice += accessoryPrice * (request.AccessoryQuantity ?? 0);
                totalQuantity += request.AccessoryQuantity ?? 0;
            }

            if (request.TerrariumVariantId.HasValue)
            {
                var variantPrice = await GetUnitPriceAsync(null, request.TerrariumVariantId);
                totalPrice += variantPrice * (request.VariantQuantity ?? 0);
                totalQuantity += request.VariantQuantity ?? 0;
            }

            cartItem = new CartItem
            {
                CartId = cart.CartId,
                AccessoryId = request.AccessoryId,
                TerrariumVariantId = request.TerrariumVariantId,
                AccessoryQuantity = request.AccessoryQuantity ?? 0,
                TerrariumVariantQuantity = request.VariantQuantity ?? 0,
                Quantity = totalQuantity,
                TotalPrice = totalPrice,
                UnitPrice = totalQuantity > 0 ? totalPrice / totalQuantity : 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.CartItemRepository.CreateAsync(cartItem);
        }

        // Tạo response để trả về cho client
        var response = new CartItemResponse
        {
            CartItemId = cartItem.CartItemId,
            CartId = cart.CartId,
            CreatedAt = cartItem.CreatedAt,
            UpdatedAt = cartItem.UpdatedAt,
            Item = new List<CartItemDetail>(),
            TotalCartQuantity = cartItem.Quantity,
            TotalCartPrice = cartItem.TotalPrice
        };

        // Thêm Accessory vào response
        if (cartItem.AccessoryId.HasValue)
        {
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
            response.Item.Add(new CartItemDetail
            {
                ProductName = accessory?.Name,
                Quantity = cartItem.AccessoryQuantity ?? 0,
                Price = cartItem.UnitPrice,
                TotalPrice = (cartItem.AccessoryQuantity ?? 0) * cartItem.UnitPrice
            });
        }

        // Thêm Terrarium vào response
        if (cartItem.TerrariumVariantId.HasValue)
        {
            var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
            response.Item.Add(new CartItemDetail
            {
                ProductName = $"Terrarium {variant?.VariantName}",
                Quantity = cartItem.TerrariumVariantQuantity ?? 0,
                Price = cartItem.UnitPrice,
                TotalPrice = (cartItem.TerrariumVariantQuantity ?? 0) * cartItem.UnitPrice
            });
        }

        return response;
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




    public async Task<IBusinessResult> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        var cartItem = await _unitOfWork.CartItemRepository
        .Include(ci => ci.Cart)
        .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

        if (cartItem == null)
            return new BusinessResult(Const.FAIL_READ_CODE, $"Cart for CartItem with ID {cartItemId} not found.");

        if (cartItem.Cart == null)
            return new BusinessResult(Const.FAIL_READ_CODE, $"Cart for CartItem with ID {cartItemId} not found."); 
             

        if (cartItem.Cart.UserId != userId)
            return new BusinessResult(Const.FAIL_READ_CODE, "You do not have permission to update this cart item.");

        var cartItemResponse = new CartItemResponse
        {
            CartItemId = cartItem.CartItemId,
            CartId = cartItem.CartId,
            AccessoryId = cartItem.AccessoryId,
            TerrariumVariantId = cartItem.TerrariumVariantId,
            Item = new List<CartItemDetail>(),
            CreatedAt = cartItem.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        decimal totalCartPrice = 0;
        int totalCartQuantity = 0;

        // Cập nhật Accessory nếu có
        if (cartItem.AccessoryId.HasValue && request.AccessoryQuantity.HasValue && request.AccessoryQuantity.Value > 0)
        {
            decimal unitPrice = await GetUnitPriceAsync(cartItem.AccessoryId, null);
            cartItem.AccessoryQuantity = request.AccessoryQuantity.Value;

            var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
            var accessoryDetail = new CartItemDetail
            {
                ProductName = accessory.Name,
                Quantity = request.AccessoryQuantity.Value,
                Price = unitPrice,
                TotalPrice = unitPrice * request.AccessoryQuantity.Value
            };

            cartItemResponse.Item.Add(accessoryDetail);
            totalCartPrice += accessoryDetail.TotalPrice;
            totalCartQuantity += accessoryDetail.Quantity;
        }
        else
        {
            cartItem.AccessoryQuantity = null;
        }

        // Cập nhật Terrarium nếu có
        if (cartItem.TerrariumVariantId.HasValue && request.VariantQuantity.HasValue && request.VariantQuantity.Value > 0)
        {
            decimal unitPrice = await GetUnitPriceAsync(null, cartItem.TerrariumVariantId);
            cartItem.TerrariumVariantQuantity = request.VariantQuantity.Value;

            var terrarium = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
            var terrariumDetail = new CartItemDetail
            {
                ProductName = $"Terrarium {terrarium.VariantName}",
                Quantity = request.VariantQuantity.Value,
                Price = unitPrice,
                TotalPrice = unitPrice * request.VariantQuantity.Value
            };

            cartItemResponse.Item.Add(terrariumDetail);
            totalCartPrice += terrariumDetail.TotalPrice;
            totalCartQuantity += terrariumDetail.Quantity;
        }
        else
        {
            cartItem.TerrariumVariantQuantity = null;
        }

        // Nếu không còn sản phẩm nào => không update
        if (totalCartQuantity == 0)
            throw new InvalidOperationException("You must keep at least one item with quantity greater than zero.");

        // Cập nhật lại tổng giá và số lượng
        cartItem.Quantity = totalCartQuantity;
        cartItem.TotalPrice = totalCartPrice;
        cartItem.UnitPrice = totalCartQuantity > 0 ? totalCartPrice / totalCartQuantity : 0;
        cartItem.UpdatedAt = DateTime.UtcNow;

        cartItemResponse.TotalCartPrice = totalCartPrice;
        cartItemResponse.TotalCartQuantity = totalCartQuantity;

        await _unitOfWork.CartItemRepository.UpdateAsync(cartItem);

        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, cartItemResponse);
    }






    public async Task<bool> RemoveItemAsync(int userId, int itemId)
    {
        var cartItem = await _unitOfWork.CartItemRepository.GetByIdWithCartAsync(itemId);
        if (cartItem == null || cartItem.Cart.UserId != userId)
            return false;

        await _unitOfWork.CartItemRepository.RemoveAsync(cartItem);
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

    public async Task<IBusinessResult> CheckoutAsync(int userId)
    {
        try
        {
            // Lấy giỏ hàng của người dùng
            var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
                return new BusinessResult(Const.FAIL_READ_CODE, "Giỏ hàng trống hoặc không tồn tại.");

            decimal totalCartPrice = 0;
            int totalCartQuantity = 0;


        // Tạo đối tượng phản hồi cho đơn hàng
        var orderResponse = new OrderResponse
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            PaymentStatus = "Unpaid",
            ShippingStatus = "Unprocessed",
            OrderItems = new List<OrderItemResponse>(),
        };

        var order = new Order
        {
            UserId = userId,
            TotalAmount = totalCartPrice, // Sẽ tính lại sau
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            PaymentStatus = "Unpaid",
            ShippingStatus = "Unprocessed",
            OrderItems = new List<OrderItem>(),
        };


            // Xử lý từng mục trong giỏ hàng để chuyển thành đơn hàng
            foreach (var cartItem in cart.CartItems)
            {
                decimal itemTotalPrice = 0;
                int itemQuantity = 0;

                // Xử lý Accessory nếu có
                if (cartItem.AccessoryId.HasValue && cartItem.AccessoryQuantity.HasValue)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                    if (accessory != null)
                    {
                        decimal unitPrice = accessory.Price;
                        int quantity = cartItem.AccessoryQuantity.Value;
                        itemTotalPrice = unitPrice * quantity;
                        itemQuantity = quantity;

                        // Thêm vào danh sách OrderItem
                        order.OrderItems.Add(new OrderItem
                        {
                            AccessoryId = cartItem.AccessoryId,
                            AccessoryQuantity = cartItem.AccessoryQuantity,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = itemTotalPrice
                        });
                    }
                }

                // Xử lý Terrarium nếu có
                if (cartItem.TerrariumVariantId.HasValue && cartItem.TerrariumVariantQuantity.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
                    if (variant != null)
                    {
                        decimal unitPrice = variant.Price;
                        int quantity = cartItem.TerrariumVariantQuantity.Value;
                        itemTotalPrice = unitPrice * quantity;
                        itemQuantity = quantity;

                        // Thêm vào danh sách OrderItem
                        order.OrderItems.Add(new OrderItem
                        {
                            TerrariumVariantId = cartItem.TerrariumVariantId,
                            TerrariumVariantQuantity = cartItem.TerrariumVariantQuantity,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = itemTotalPrice
                        });
                    }
                }

                totalCartPrice += itemTotalPrice; // Cộng vào tổng giá trị của đơn hàng
                totalCartQuantity += itemQuantity; // Cộng vào tổng số lượng
            }

            order.TotalAmount = totalCartPrice; // Cập nhật tổng giá trị đơn hàng

            // Lưu đơn hàng vào cơ sở dữ liệu
            await _unitOfWork.OrderRepository.CreateAsync(order);

            // Lưu các mục cần xóa vào danh sách tạm
            var cartItemsToRemove = cart.CartItems.ToList(); // ToList() để tạo một bản sao tách biệt của CartItems
                                                             // Xóa các mục đã thanh toán từ giỏ hàng
            foreach (var cartItem in cartItemsToRemove)
            {
                await _unitOfWork.CartItemRepository.RemoveAsync(cartItem);
            }
            await _unitOfWork.SaveAsync();

            // Cập nhật lại thông tin phản hồi đơn hàng
            orderResponse.OrderId = order.OrderId;
            orderResponse.TotalAmount = totalCartPrice;

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Checkout thành công", orderResponse);
        }
        catch (Exception ex)
        {
            // Log lỗi và throw lại exception để có thể xử lý ở ngoài
            // Ghi log chi tiết tại đây
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
}