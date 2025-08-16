using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.ResponseModel.Cart;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using static TerrariumGardenTech.Common.Enums.CommonData;


namespace TerrariumGardenTech.Service.Service;

public class CartService : ICartService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartService(UnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        
        var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Cart.CreateAsync(cart);
        }
        else
        {
            Console.WriteLine($"Found existing cart with ID {cart.CartId} for user with ID {userId}.");
        }

        return cart;
    }

    public async Task<Cart> GetCartByUserAsync(int userId)
    {
        return await _unitOfWork.Cart.GetByUserIdAsync(userId);
    }

    /// <summary>
    /// Lấy thông tin giỏ hàng với phân loại bundle và single items
    /// </summary>
    public async Task<IBusinessResult> GetCartAsync(int userId)
    {
        var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
        if (cart == null)
            return new BusinessResult(Const.FAIL_READ_CODE, "Không có giỏ hàng");

        var user = await _unitOfWork.User.GetByIdAsync(userId);
        if (user == null)
            return new BusinessResult(Const.FAIL_READ_CODE, $"User with ID {userId} not found.");

        var cartResponse = new CartResponse
        {
            CartId = cart.CartId,
            UserId = userId,
            User = user.Email,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };

        // Lấy các item chính (bể thủy sinh)
        var mainItems = cart.CartItems
            .Where(x => x.ItemType == CartItemType.MAIN_ITEM)
            .ToList();

        // Lấy các item đơn lẻ
        var singleItems = cart.CartItems
            .Where(x => x.ItemType == CartItemType.SINGLE)
            .ToList();

        // === XỬ LÝ CÁC BUNDLE (BỂ + PHỤ KIỆN) ===
        foreach (var mainItem in mainItems)
        {
            var bundleResponse = new CartBundleResponse
            {
                MainItem = await BuildCartItemResponseAsync(mainItem)
            };

            // Lấy phụ kiện thuộc bundle này
            var bundleAccessories = cart.CartItems
                .Where(x => x.ParentCartItemId == mainItem.CartItemId &&
                           x.ItemType == CartItemType.BUNDLE_ACCESSORY)
                .ToList();

            foreach (var accessory in bundleAccessories)
            {
                var accessoryResponse = await BuildCartItemResponseAsync(accessory);
                bundleResponse.BundleAccessories.Add(accessoryResponse);
            }

            bundleResponse.UpdateTotals();
            cartResponse.BundleItems.Add(bundleResponse);
        }

        // === XỬ LÝ CÁC ITEM ĐƠN LẺ ===
        foreach (var singleItem in singleItems)
        {
            var response = await BuildCartItemResponseAsync(singleItem);
            cartResponse.SingleItems.Add(response);
        }

        // === TÍNH TỔNG GIỎ HÀNG ===
        cartResponse.TotalCartPrice = cartResponse.BundleItems.Sum(x => x.TotalBundlePrice) +
                                     cartResponse.SingleItems.Sum(x => x.TotalCartPrice);

        cartResponse.TotalCartQuantity = cartResponse.BundleItems.Sum(x => x.TotalBundleQuantity) +
                                        cartResponse.SingleItems.Sum(x => x.TotalCartQuantity);

        cartResponse.TotalCartItem = cartResponse.BundleItems.Count + cartResponse.SingleItems.Count;

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, cartResponse);
    }

    /// <summary>
    /// Thêm sản phẩm vào giỏ hàng
    /// Hỗ trợ:
    /// - Thêm bể đơn lẻ: TerrariumVariantId + VariantQuantity
    /// - Thêm phụ kiện đơn lẻ: AccessoryId + AccessoryQuantity  
    /// - Thêm bể + combo: TerrariumVariantId + VariantQuantity + BundleAccessories
    /// </summary>
    public async Task<CartBundleResponse> AddItemAsync(int userId, AddCartItemRequest request)
    {
        // Validation: phải có ít nhất 1 sản phẩm
        if (!request.AccessoryId.HasValue && !request.TerrariumVariantId.HasValue)
            throw new ArgumentException("Phải truyền AccessoryId hoặc TerrariumVariantId.");

        var cart = await GetOrCreateCartAsync(userId);
        var bundleResponse = new CartBundleResponse();

        if (request.TerrariumVariantId.HasValue)
        {
            // === TRƯỜNG HỢP 1: THÊM BỂ THỦY SINH ===
            var mainItem = await AddTerrariumToCartAsync(cart, request);
            bundleResponse.MainItem = await BuildCartItemResponseAsync(mainItem);

            // Thêm phụ kiện kèm theo (nếu có)
            if (request.BundleAccessories?.Any() == true)
            {
                foreach (var bundleAccessory in request.BundleAccessories)
                {
                    var accessoryItem = await AddBundleAccessoryToCartAsync(
                        cart,
                        mainItem.CartItemId,
                        bundleAccessory
                    );

                    var accessoryResponse = await BuildCartItemResponseAsync(accessoryItem);
                    bundleResponse.BundleAccessories.Add(accessoryResponse);
                }
            }
        }
        else if (request.AccessoryId.HasValue)
        {
            // === TRƯỜNG HỢP 2: THÊM PHỤ KIỆN ĐỒN LẺ ===
            var singleItem = await AddSingleAccessoryToCartAsync(cart, request);
            bundleResponse.MainItem = await BuildCartItemResponseAsync(singleItem);
        }

        // Tính tổng
        bundleResponse.UpdateTotals();
        return bundleResponse;
    }

    /// <summary>
    /// Thêm bể thủy sinh vào giỏ hàng
    /// ItemType = "MAIN_ITEM"
    /// </summary>
    private async Task<CartItem> AddTerrariumToCartAsync(Cart cart, AddCartItemRequest request)
    {
        // Kiểm tra đã có bể này trong giỏ chưa
        var existingItem = await _unitOfWork.CartItem.GetExistingItemAsync(
            cart.CartId,
            null,
            request.TerrariumVariantId
        );

        if (existingItem != null && existingItem.ItemType == CartItemType.MAIN_ITEM)
        {
            // Cập nhật số lượng bể hiện có
            existingItem.TerrariumVariantQuantity += request.VariantQuantity ?? 1;
            existingItem.Quantity = existingItem.TerrariumVariantQuantity ?? 0;

            var unitPrice = await GetUnitPriceAsync(null, request.TerrariumVariantId);
            existingItem.TotalPrice = unitPrice * existingItem.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CartItem.UpdateAsync(existingItem);
            return existingItem;
        }

        // Tạo mới bể
        var quantity = request.VariantQuantity ?? 1;
        var price = await GetUnitPriceAsync(null, request.TerrariumVariantId);

        var cartItem = new CartItem
        {
            CartId = cart.CartId,
            TerrariumVariantId = request.TerrariumVariantId,
            TerrariumVariantQuantity = quantity,
            Quantity = quantity,
            UnitPrice = price,
            TotalPrice = price * quantity,
            ItemType = CartItemType.MAIN_ITEM, // Đánh dấu là bể chính
            ParentCartItemId = null,           // Không có cha
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CartItem.CreateAsync(cartItem);
        return cartItem;
    }

    /// <summary>
    /// Thêm phụ kiện thuộc combo bể thủy sinh
    /// ItemType = "BUNDLE_ACCESSORY"
    /// </summary>
    private async Task<CartItem> AddBundleAccessoryToCartAsync(
        Cart cart,
        int parentItemId,
        BundleAccessoryRequest bundleAccessory)
    {
        var unitPrice = await GetUnitPriceAsync(bundleAccessory.AccessoryId, null);
        var quantity = bundleAccessory.Quantity;

        var cartItem = new CartItem
        {
            CartId = cart.CartId,
            AccessoryId = bundleAccessory.AccessoryId,
            AccessoryQuantity = quantity,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * quantity,
            ParentCartItemId = parentItemId,              // Thuộc về bể nào
            ItemType = CartItemType.BUNDLE_ACCESSORY,     // Phụ kiện bundle
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CartItem.CreateAsync(cartItem);
        return cartItem;
    }

    /// <summary>
    /// Thêm phụ kiện đơn lẻ (không thuộc bundle nào)
    /// ItemType = "SINGLE"
    /// </summary>
    private async Task<CartItem> AddSingleAccessoryToCartAsync(Cart cart, AddCartItemRequest request)
    {
        // Kiểm tra đã có phụ kiện này trong giỏ chưa (loại SINGLE)
        var existingItem = await _unitOfWork.CartItem.GetExistingItemAsync(
            cart.CartId,
            request.AccessoryId,
            null
        );

        if (existingItem != null && existingItem.ItemType == CartItemType.SINGLE)
        {
            // Cập nhật số lượng
            existingItem.AccessoryQuantity += request.AccessoryQuantity ?? 1;
            existingItem.Quantity = existingItem.AccessoryQuantity ?? 0;

            var unitPrice = await GetUnitPriceAsync(request.AccessoryId, null);
            existingItem.TotalPrice = unitPrice * existingItem.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CartItem.UpdateAsync(existingItem);
            return existingItem;
        }

        // Tạo mới
        var quantity = request.AccessoryQuantity ?? 1;
        var price = await GetUnitPriceAsync(request.AccessoryId, null);

        var cartItem = new CartItem
        {
            CartId = cart.CartId,
            AccessoryId = request.AccessoryId,
            AccessoryQuantity = quantity,
            Quantity = quantity,
            UnitPrice = price,
            TotalPrice = price * quantity,
            ItemType = CartItemType.SINGLE,  // Sản phẩm đơn lẻ
            ParentCartItemId = null,         // Không thuộc bundle
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CartItem.CreateAsync(cartItem);
        return cartItem;
    }

    /// <summary>
    /// 🔑 PHƯƠNG THỨC QUAN TRỌNG: Build response cho CartItem
    /// </summary>
    private async Task<CartItemResponse> BuildCartItemResponseAsync(CartItem cartItem)
    {
        var response = new CartItemResponse
        {
            CartItemId = cartItem.CartItemId,
            CartId = cartItem.CartId,
            AccessoryId = cartItem.AccessoryId,
            TerrariumVariantId = cartItem.TerrariumVariantId,
            Item = new List<CartItemDetail>(),
            CreatedAt = cartItem.CreatedAt,
            UpdatedAt = cartItem.UpdatedAt
        };

        decimal totalPrice = 0m;
        int totalQuantity = 0;

        // Build chi tiết cho Accessory
        if (cartItem.AccessoryId.HasValue && (cartItem.AccessoryQuantity ?? 0) > 0)
        {
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
            if (accessory != null)
            {
                var qty = cartItem.AccessoryQuantity!.Value;
                var price = accessory.Price;

                response.Item.Add(new CartItemDetail
                {
                    ProductName = accessory.Name,
                    Quantity = qty,
                    Price = price,
                    TotalPrice = price * qty
                });

                totalPrice += price * qty;
                totalQuantity += qty;
            }
        }

        // Build chi tiết cho Terrarium Variant
        if (cartItem.TerrariumVariantId.HasValue && (cartItem.TerrariumVariantQuantity ?? 0) > 0)
        {
            var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
            if (variant != null)
            {
                var qty = cartItem.TerrariumVariantQuantity!.Value;
                var price = variant.Price;

                response.Item.Add(new CartItemDetail
                {
                    ProductName = $"Terrarium {variant.VariantName}",
                    Quantity = qty,
                    Price = price,
                    TotalPrice = price * qty
                });

                totalPrice += price * qty;
                totalQuantity += qty;
            }
        }

        response.TotalCartPrice = totalPrice;
        response.TotalCartQuantity = totalQuantity;

        return response;
    }

    private async Task<decimal> GetUnitPriceAsync(int? accessoryId, int? terrariumVariantId)
    {
        if (accessoryId.HasValue)
        {
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(accessoryId.Value);
            return accessory?.Price ?? 0;
        }

        if (terrariumVariantId.HasValue)
        {
            var terrariumVariant = await _unitOfWork.TerrariumVariant.GetByIdAsync(terrariumVariantId.Value);
            return terrariumVariant?.Price ?? 0;
        }

        return 0;
    }

    public async Task<IBusinessResult> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        var cartItem = await _unitOfWork.CartItem
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

        if (cartItem == null)
            return new BusinessResult(Const.FAIL_READ_CODE, $"CartItem with ID {cartItemId} not found.");

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

        await _unitOfWork.CartItem.UpdateAsync(cartItem);

        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, cartItemResponse);
    }

    public async Task<bool> RemoveItemAsync(int userId, int itemId)
    {
        var cartItem = await _unitOfWork.CartItem.GetByIdWithCartAsync(itemId);
        if (cartItem == null || cartItem.Cart.UserId != userId)
            return false;

        await _unitOfWork.CartItem.RemoveAsync(cartItem);
        return true;
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
        if (cart == null)
            return false;

        await _unitOfWork.Cart.ClearAsync(userId);
        return true;
    }
    

    public async Task<IBusinessResult> CheckoutAsync(int userId)
    {
        try
        {
            var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
                return new BusinessResult(Const.FAIL_READ_CODE, "Giỏ hàng trống hoặc không tồn tại.");

            decimal totalCartPrice = 0;

            var defaultAddress = await _unitOfWork.Address.GetDefaultByUserIdAsync(userId);
            if (defaultAddress == null)
                return new BusinessResult(Const.BAD_REQUEST_CODE, "Vui lòng thêm địa chỉ trước khi thanh toán.");

            var order = new Order
            {
                UserId = userId,
                AddressId = defaultAddress.AddressId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatusEnum.Pending,
                PaymentStatus = "Unpaid",
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cart.CartItems)
            {
                if (cartItem.AccessoryId.HasValue && cartItem.AccessoryQuantity.HasValue)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                    if (accessory != null)
                    {
                        var quantity = cartItem.AccessoryQuantity ?? 0;
                        var unitPrice = accessory.Price;
                        var totalPrice = unitPrice * quantity;

                        order.OrderItems.Add(new OrderItem
                        {
                            AccessoryId = cartItem.AccessoryId,
                            AccessoryQuantity = quantity,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = totalPrice
                        });

                        totalCartPrice += totalPrice;
                    }
                }

                if (cartItem.TerrariumVariantId.HasValue && cartItem.TerrariumVariantQuantity.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
                    if (variant != null)
                    {
                        var quantity = cartItem.TerrariumVariantQuantity ?? 0;
                        var unitPrice = variant.Price;
                        var totalPrice = unitPrice * quantity;

                        order.OrderItems.Add(new OrderItem
                        {
                            TerrariumVariantId = cartItem.TerrariumVariantId,
                            TerrariumVariantQuantity = quantity,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            TotalPrice = totalPrice
                        });

                        totalCartPrice += totalPrice;
                    }
                }
            }

            order.TotalAmount = totalCartPrice;
            await _unitOfWork.Order.CreateAsync(order);

            foreach (var cartItem in cart.CartItems.ToList())
            {
                await _unitOfWork.CartItem.RemoveAsync(cartItem);
            }

            await _unitOfWork.SaveAsync();

            // Map sang OrderResponse bằng AutoMapper (nếu có cấu hình)
            var orderResponse = _mapper.Map<OrderResponse>(order);

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Checkout thành công", orderResponse);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
}