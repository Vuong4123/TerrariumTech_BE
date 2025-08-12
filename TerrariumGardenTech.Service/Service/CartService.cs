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


namespace TerrariumGardenTech.Service.Service;

public class CartService : ICartService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly IMapper _mapper; // Assuming you are using AutoMapper for mapping entities to response models

    public CartService(UnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper; // Injecting AutoMapper for mapping entities to response models
    }

    public async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        Console.WriteLine($"Attempting to fetch cart for user with ID {userId}");

        var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);

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
            await _unitOfWork.Cart.CreateAsync(cart);
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
        // 1) Lấy giỏ hàng
        var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
        if (cart == null)
            return new BusinessResult(Const.FAIL_READ_CODE, "không có giỏ hàng");

        // 2) Lấy user (để hiển thị email)
        var user = await _unitOfWork.User.GetByIdAsync(userId);
        if (user == null)
            return new BusinessResult(Const.FAIL_READ_CODE, $"User with ID {userId} not found.");

        // 3) Khởi tạo response
        var cartResponse = new CartResponse
        {
            CartId = cart.CartId,
            UserId = userId,
            User = user.Email,
            CartItems = new List<CartItemResponse>(),
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };

        decimal totalCartPrice = 0m; // tổng tiền của cả giỏ
        int totalCartQuantity = 0;  // tổng SL (dùng ở dưới)
        int totalCartItem = 0;  // tổng SL món (field bạn yêu cầu)

        // 4) Duyệt từng item trong giỏ
        foreach (var cartItem in cart.CartItems)
        {
            var itemResp = new CartItemResponse
            {
                CartItemId = cartItem.CartItemId,
                CartId = cartItem.CartId,
                AccessoryId = cartItem.AccessoryId,
                TerrariumVariantId = cartItem.TerrariumVariantId,
                Item = new List<CartItemDetail>(),
                CreatedAt = cartItem.CreatedAt,
                UpdatedAt = cartItem.UpdatedAt
            };

            decimal itemTotalPrice = 0m;
            int itemQuantity = 0;

            // ---- Accessory ----
            if (cartItem.AccessoryId.HasValue && (cartItem.AccessoryQuantity ?? 0) > 0)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                if (accessory != null)
                {
                    var qty = cartItem.AccessoryQuantity!.Value;
                    var price = accessory.Price;

                    itemResp.Item.Add(new CartItemDetail
                    {
                        ProductName = accessory.Name,
                        Quantity = qty,
                        Price = price,
                        TotalPrice = price * qty
                    });

                    itemTotalPrice += price * qty;
                    itemQuantity += qty;
                }
            }

            // ---- Terrarium Variant ----
            if (cartItem.TerrariumVariantId.HasValue && (cartItem.TerrariumVariantQuantity ?? 0) > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var qty = cartItem.TerrariumVariantQuantity!.Value;
                    var price = variant.Price;

                    itemResp.Item.Add(new CartItemDetail
                    {
                        ProductName = $"Terrarium {variant.VariantName}",
                        Quantity = qty,
                        Price = price,
                        TotalPrice = price * qty
                    });

                    itemTotalPrice += price * qty;
                    itemQuantity += qty;
                }
            }

            // Gán tổng cho từng cartItem
            itemResp.TotalCartPrice = itemTotalPrice;
            itemResp.TotalCartQuantity = itemQuantity;

            // Cộng dồn cho giỏ
            cartResponse.CartItems.Add(itemResp);
            totalCartPrice += itemTotalPrice;
            totalCartQuantity += itemQuantity;
            totalCartItem += itemQuantity; // tổng số lượng món (đúng yêu cầu)
        }

        // 5) Tổng của giỏ
        cartResponse.TotalCartPrice = totalCartPrice;
        cartResponse.TotalCartQuantity = totalCartQuantity;
        cartResponse.TotalCartItem = totalCartItem;   // <— FIELD MỚI TRONG DTO

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, cartResponse);
    }


    public async Task<Cart> GetCartByUserAsync(int userId)
    {
        return await _unitOfWork.Cart.GetByUserIdAsync(userId);
    }

    public async Task<CartItemResponse> AddItemAsync(int userId, AddCartItemRequest request)
    {
        // Validate: phải có 1 trong 2 loại
        if (!request.AccessoryId.HasValue && !request.TerrariumVariantId.HasValue)
            throw new ArgumentException("Phải truyền AccessoryId hoặc TerrariumVariantId.");

        // ép mutual-exclusive (nếu truyền cả 2 thì ưu tiên accessory, bạn đổi rule nếu muốn)
        if (request.AccessoryId.HasValue)
        {
            request.TerrariumVariantId = null;
            request.VariantQuantity = null;
        }
        else if (request.TerrariumVariantId.HasValue)
        {
            request.AccessoryId = null;
            request.AccessoryQuantity = null;
        }

        var cart = await GetOrCreateCartAsync(userId);

        // tìm item trùng trong giỏ theo cặp key
        var existingItem = await _unitOfWork.CartItem.GetExistingItemAsync(
            cart.CartId,
            request.AccessoryId,
            request.TerrariumVariantId
        );

        var now = DateTime.UtcNow;
        CartItem cartItem;

        // ===== UPDATE ITEM ĐÃ CÓ =====
        if (existingItem != null)
        {
            // Tăng số lượng đúng nhánh
            if (request.AccessoryId.HasValue)
            {
                var qtyAdd = Math.Max(0, request.AccessoryQuantity ?? 0);
                existingItem.AccessoryQuantity = (existingItem.AccessoryQuantity ?? 0) + qtyAdd;

                // tính lại tiền nhánh accessory
                var unit = await GetUnitPriceAsync(existingItem.AccessoryId, null);
                var sub = unit * (existingItem.AccessoryQuantity ?? 0);

                // cộng gộp với nhánh variant (nếu item này hiện đang có cả 2 – hiếm, nhưng vẫn xử lý)
                decimal total = sub;
                if (existingItem.TerrariumVariantId.HasValue && (existingItem.TerrariumVariantQuantity ?? 0) > 0)
                {
                    var vUnit = await GetUnitPriceAsync(null, existingItem.TerrariumVariantId);
                    total += vUnit * (existingItem.TerrariumVariantQuantity ?? 0);
                }

                existingItem.TotalPrice = total;
            }
            else // Terrarium variant
            {
                var qtyAdd = Math.Max(0, request.VariantQuantity ?? 0);
                existingItem.TerrariumVariantQuantity = (existingItem.TerrariumVariantQuantity ?? 0) + qtyAdd;

                var vUnit = await GetUnitPriceAsync(null, existingItem.TerrariumVariantId);
                var vSub = vUnit * (existingItem.TerrariumVariantQuantity ?? 0);

                decimal total = vSub;
                if (existingItem.AccessoryId.HasValue && (existingItem.AccessoryQuantity ?? 0) > 0)
                {
                    var aUnit = await GetUnitPriceAsync(existingItem.AccessoryId, null);
                    total += aUnit * (existingItem.AccessoryQuantity ?? 0);
                }

                existingItem.TotalPrice = total;
            }

            // Tổng quantity (hai nhánh cộng lại)
            existingItem.Quantity =
                (existingItem.AccessoryQuantity ?? 0) + (existingItem.TerrariumVariantQuantity ?? 0);

            // UnitPrice chỉ để tham khảo; không dùng để hiển thị từng nhánh
            existingItem.UnitPrice = existingItem.Quantity > 0
                ? existingItem.TotalPrice / existingItem.Quantity
                : 0;

            existingItem.UpdatedAt = now;

            await _unitOfWork.CartItem.UpdateAsync(existingItem);
            cartItem = existingItem;
        }
        // ===== TẠO MỚI ITEM =====
        else
        {
            decimal totalPrice = 0m;
            int totalQty = 0;

            int? accQty = null;
            int? varQty = null;

            if (request.AccessoryId.HasValue)
            {
                var qty = Math.Max(0, request.AccessoryQuantity ?? 0);
                var price = await GetUnitPriceAsync(request.AccessoryId, null);
                totalPrice += price * qty;
                totalQty += qty;
                accQty = qty;
            }
            else if (request.TerrariumVariantId.HasValue)
            {
                var qty = Math.Max(0, request.VariantQuantity ?? 0);
                var price = await GetUnitPriceAsync(null, request.TerrariumVariantId);
                totalPrice += price * qty;
                totalQty += qty;
                varQty = qty;
            }

            cartItem = new CartItem
            {
                CartId = cart.CartId,

                // set đúng nhánh, nhánh còn lại để null
                AccessoryId = request.AccessoryId,
                AccessoryQuantity = accQty,

                TerrariumVariantId = request.TerrariumVariantId,
                TerrariumVariantQuantity = varQty,

                Quantity = totalQty,
                TotalPrice = totalPrice,
                UnitPrice = totalQty > 0 ? totalPrice / totalQty : 0,

                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.CartItem.CreateAsync(cartItem);
        }

        // ===== TRẢ RESPONSE =====
        var resp = new CartItemResponse
        {
            CartItemId = cartItem.CartItemId,
            CartId = cart.CartId,
            AccessoryId = cartItem.AccessoryId,
            TerrariumVariantId = cartItem.TerrariumVariantId,
            CreatedAt = cartItem.CreatedAt,
            UpdatedAt = cartItem.UpdatedAt,
            Item = new List<CartItemDetail>(),
            TotalCartQuantity = cartItem.Quantity,
            TotalCartPrice = cartItem.TotalPrice
        };

        // Build chi tiết đúng đơn giá từng nhánh (KHÔNG dùng UnitPrice trung bình)
        if (cartItem.AccessoryId.HasValue && (cartItem.AccessoryQuantity ?? 0) > 0)
        {
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
            var unit = await GetUnitPriceAsync(cartItem.AccessoryId, null);

            resp.Item.Add(new CartItemDetail
            {
                ProductName = accessory?.Name,
                Quantity = cartItem.AccessoryQuantity ?? 0,
                Price = unit,
                TotalPrice = unit * (cartItem.AccessoryQuantity ?? 0)
            });
        }

        if (cartItem.TerrariumVariantId.HasValue && (cartItem.TerrariumVariantQuantity ?? 0) > 0)
        {
            var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
            var unit = await GetUnitPriceAsync(null, cartItem.TerrariumVariantId);

            resp.Item.Add(new CartItemDetail
            {
                ProductName = $"Terrarium {variant?.VariantName}",
                Quantity = cartItem.TerrariumVariantQuantity ?? 0,
                Price = unit,
                TotalPrice = unit * (cartItem.TerrariumVariantQuantity ?? 0)
            });
        }

        return resp;
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
        var cartItem = await _unitOfWork.CartItem
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