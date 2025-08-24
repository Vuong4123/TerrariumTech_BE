using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using static TerrariumGardenTech.Common.Enums.CommonData;

namespace TerrariumGardenTech.Service.Service
{
    public class CartService : ICartService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(UnitOfWork unitOfWork, IMapper mapper, ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Cart Management

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
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Created new cart for user {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("Found existing cart with ID {CartId} for user {UserId}", cart.CartId, userId);
            }

            return cart;
        }

        public async Task<Cart> GetCartByUserAsync(int userId)
        {
            return await _unitOfWork.Cart.GetByUserIdAsync(userId);
        }

        #endregion

        #region Get Cart

        public async Task<IBusinessResult> GetCartAsync(int userId)
        {
            try
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
                    UpdatedAt = cart.UpdatedAt,
                    BundleItems = new List<CartBundleResponse>(),
                    SingleItems = new List<CartItemResponse>()
                };

                // Lấy các item chính (bể thủy sinh - MAIN_ITEM)
                var mainItems = cart.CartItems
                    .Where(x => x.ItemType == CartItemType.MAIN_ITEM)
                    .ToList();

                // Lấy các item đơn lẻ (SINGLE)
                var singleItems = cart.CartItems
                    .Where(x => x.ItemType == CartItemType.SINGLE)
                    .ToList();

                // === XỬ LÝ CÁC BUNDLE (BỂ + PHỤ KIỆN) ===
                foreach (var mainItem in mainItems)
                {
                    var bundleResponse = new CartBundleResponse
                    {
                        MainItem = await BuildCartItemResponseAsync(mainItem),
                        BundleAccessories = new List<CartItemResponse>()
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

                // === XỬ LÝ CÁC COMBO ===
                var comboItems = cart.CartItems
                    .Where(x => x.ItemType == CartItemType.COMBO)
                    .ToList();

                foreach (var comboItem in comboItems)
                {
                    var comboResponse = await BuildComboCartItemResponseAsync(comboItem);
                    cartResponse.SingleItems.Add(comboResponse);
                }

                // === TÍNH TỔNG GIỎ HÀNG ===
                cartResponse.TotalCartPrice = cartResponse.BundleItems.Sum(x => x.TotalBundlePrice) +
                                             cartResponse.SingleItems.Sum(x => x.TotalCartPrice);

                cartResponse.TotalCartQuantity = cartResponse.BundleItems.Sum(x => x.TotalBundleQuantity) +
                                                cartResponse.SingleItems.Sum(x => x.TotalCartQuantity);

                cartResponse.TotalCartItem = cartResponse.BundleItems.Count + cartResponse.SingleItems.Count;

                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, cartResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy giỏ hàng: {ex.Message}");
            }
        }

        #endregion

        #region Add Items

        //public async Task<IBusinessResult> AddItemAsync(int userId, AddCartItemRequest request)
        //{
        //    try
        //    {
        //        var cart = await GetOrCreateCartAsync(userId);
        //        var bundleResponse = new CartBundleResponse();
        //        if (request.TerrariumId > 0)
        //        {
        //            // === TRƯỜNG HỢP 1: THÊM BỂ THỦY SINH ===
        //            var mainItem = await AddTerrariumToCartAsync(cart, request);
        //            bundleResponse.MainItem = await BuildCartItemResponseAsync(mainItem);
        //        }
        //        else if (request.AccessoryId.HasValue)
        //        {
        //            // === TRƯỜNG HỢP 2: THÊM PHỤ KIỆN ĐƠN LẺ ===
        //            var singleItem = await AddSingleAccessoryToCartAsync(cart, request);
        //            bundleResponse.MainItem = await BuildCartItemResponseAsync(singleItem);
        //        }

        //        await _unitOfWork.SaveAsync();

        //        // Tính tổng
        //        bundleResponse.UpdateTotals();
        //        return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Thêm sản phẩm vào giỏ hàng thành công", bundleResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error adding item to cart for user {UserId}", userId);
        //        return new BusinessResult(Const.FAIL_CREATE_CODE, $"Lỗi khi thêm sản phẩm vào giỏ hàng: {ex.Message}");
        //    }
        //}
        public async Task<IBusinessResult> AddItemAsync(int userId, AddCartItemRequest request)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(userId);
                var bundleResponse = new CartBundleResponse();
                CartItem addedItem = null;

                if (request.TerrariumId > 0)
                {
                    // === TRƯỜNG HỢP 1: THÊM BỂ THỦY SINH ===

                    // Kiểm tra variant có tồn tại không
                    if (request.TerrariumVariantId.HasValue)
                    {
                        var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(request.TerrariumVariantId.Value);
                        if (variant == null || variant.TerrariumId != request.TerrariumId)
                        {
                            return new BusinessResult(Const.FAIL_CREATE_CODE, "Variant không hợp lệ");
                        }
                    }

                    // ✅ Kiểm tra item tồn tại với cả TerrariumId VÀ VariantId
                    var existingItem = await _unitOfWork.CartItem.GetExistingTerrariumItemAsync(
                        cart.CartId,
                        request.TerrariumId,
                        request.TerrariumVariantId
                    );

                    if (existingItem != null)
                    {
                        // Cập nhật số lượng item hiện có
                        var quantityToAdd = request.VariantQuantity ?? 1;
                        existingItem.TerrariumVariantQuantity += quantityToAdd;
                        existingItem.Quantity = existingItem.TerrariumVariantQuantity ?? 0;

                        // Cập nhật giá
                        var unitPrice = await GetUnitPriceAsync(null, existingItem.TerrariumVariantId);
                        existingItem.TotalPrice = unitPrice * existingItem.Quantity;
                        existingItem.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.CartItem.UpdateAsync(existingItem);
                        addedItem = existingItem;
                    }
                    else
                    {
                        // ✅ Tạo mới cart item
                        var quantity = request.VariantQuantity ?? 1;
                        decimal unitPrice = 0;

                        if (request.TerrariumVariantId.HasValue)
                        {
                            // Có variant cụ thể
                            var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(request.TerrariumVariantId.Value);
                            unitPrice = variant.Price;

                            var cartItem = new CartItem
                            {
                                TerrariumId = request.TerrariumId,
                                CartId = cart.CartId,
                                TerrariumVariantId = request.TerrariumVariantId.Value,
                                TerrariumVariantQuantity = quantity,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                TotalPrice = unitPrice * quantity,
                                ItemType = CartItemType.MAIN_ITEM,
                                ParentCartItemId = null,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.CartItem.CreateAsync(cartItem);
                            addedItem = cartItem;
                        }
                        else
                        {
                            // Không có variant - thêm terrarium với giá base
                            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(request.TerrariumId);
                            unitPrice = terrarium?.MinPrice ?? 0;

                            var cartItem = new CartItem
                            {
                                TerrariumId = request.TerrariumId,
                                CartId = cart.CartId,
                                TerrariumVariantId = null,
                                TerrariumVariantQuantity = quantity,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                TotalPrice = unitPrice * quantity,
                                ItemType = CartItemType.MAIN_ITEM,
                                ParentCartItemId = null,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.CartItem.CreateAsync(cartItem);
                            addedItem = cartItem;
                        }
                    }

                    //bundleResponse.MainItem = await BuildCartItemResponseAsync(addedIte m);
                }
                else if (request.AccessoryId.HasValue)
                {
                    // === TRƯỜNG HỢP 2: THÊM PHỤ KIỆN ĐƠN LẺ ===
                    var singleItem = await AddSingleAccessoryToCartAsync(cart, request);
                    bundleResponse.MainItem = await BuildCartItemResponseAsync(singleItem);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Yêu cầu không hợp lệ: Phải có TerrariumId hoặc AccessoryId");
                }

                await _unitOfWork.SaveAsync();

                // Tính tổng
                bundleResponse.UpdateTotals();
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Thêm sản phẩm vào giỏ hàng thành công", bundleResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_CREATE_CODE, $"Lỗi khi thêm sản phẩm vào giỏ hàng: {ex.Message}");
            }
        }
        public async Task<IBusinessResult> ChangeVariantAsync(int userId, int cartItemId, ChangeVariantRequest request)
        {
            try
            {
                // 1. Lấy cart item hiện tại
                var currentItem = await _unitOfWork.CartItem.GetByIdWithCartAsync(cartItemId);

                if (currentItem == null)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy sản phẩm trong giỏ hàng");

                if (currentItem.Cart.UserId != userId)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Bạn không có quyền thay đổi sản phẩm này");

                if (currentItem.TerrariumId <= 0)
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Sản phẩm này không phải là terrarium");

                // 2. Validate variant mới
                var newVariant = await _unitOfWork.TerrariumVariant.GetByIdAsync(request.NewVariantId);
                if (newVariant == null)
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Variant không tồn tại");

                if (newVariant.TerrariumId != currentItem.TerrariumId)
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Variant không thuộc về terrarium này");

                // 3. Kiểm tra xem variant mới đã tồn tại trong giỏ chưa
                var existingItemWithNewVariant = await _unitOfWork.CartItem.GetExistingItemMAIN_ITEMAsync(
                    currentItem.CartId,
                    currentItem.TerrariumId,
                    request.NewVariantId
                );

                if (existingItemWithNewVariant != null && existingItemWithNewVariant.CartItemId != cartItemId)
                {
                    // Variant mới đã tồn tại -> gộp quantity và xóa item cũ
                    var quantityToMerge = request.Quantity ?? currentItem.TerrariumVariantQuantity ?? 1;

                    // Check stock
                    if (newVariant.StockQuantity < (existingItemWithNewVariant.TerrariumVariantQuantity ?? 0) + quantityToMerge)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE,
                            $"Không đủ hàng. Chỉ còn {newVariant.StockQuantity} sản phẩm trong kho");
                    }

                    // Cộng quantity vào item đã tồn tại
                    existingItemWithNewVariant.TerrariumVariantQuantity =
                        (existingItemWithNewVariant.TerrariumVariantQuantity ?? 0) + quantityToMerge;
                    existingItemWithNewVariant.Quantity = existingItemWithNewVariant.TerrariumVariantQuantity.Value;
                    existingItemWithNewVariant.TotalPrice = newVariant.Price * existingItemWithNewVariant.Quantity;
                    existingItemWithNewVariant.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.CartItem.UpdateAsync(existingItemWithNewVariant);

                    // Xóa item cũ
                    await _unitOfWork.CartItem.RemoveAsync(currentItem);
                    await _unitOfWork.SaveAsync();

                    var mergedResponse = await BuildCartItemResponseAsync(existingItemWithNewVariant);
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE,
                        "Đã gộp sản phẩm với variant đã có trong giỏ hàng", mergedResponse);
                }
                else
                {
                    // Variant mới chưa tồn tại -> chỉ cần update
                    var newQuantity = request.Quantity ?? currentItem.TerrariumVariantQuantity ?? 1;

                    // Check stock
                    if (newVariant.StockQuantity < newQuantity)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE,
                            $"Không đủ hàng. Chỉ còn {newVariant.StockQuantity} sản phẩm trong kho");
                    }

                    // Update variant và các thông tin liên quan
                    currentItem.TerrariumVariantId = request.NewVariantId;
                    currentItem.TerrariumVariantQuantity = newQuantity;
                    currentItem.Quantity = newQuantity;
                    currentItem.UnitPrice = newVariant.Price;
                    currentItem.TotalPrice = newVariant.Price * newQuantity;
                    currentItem.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.CartItem.UpdateAsync(currentItem);
                    await _unitOfWork.SaveAsync();

                    var response = await BuildCartItemResponseAsync(currentItem);
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE,
                        "Đã thay đổi variant thành công", response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing variant for user {UserId}, item {ItemId}", userId, cartItemId);
                return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi thay đổi variant: {ex.Message}");
            }
        }


        public async Task<IBusinessResult> AddMultipleItemAsync(int userId, AddCartItemMultipleRequest request)
        {
            var cart = await GetOrCreateCartAsync(userId);
            // Kiểm tra đã có bể này trong giỏ chưa
            var existingItem = await _unitOfWork.CartItem.GetExistingTerrariumItemAsync(
                cart.CartId,
                request.TerrariumId
            );

            CartItem mainCartItem;

            if (existingItem != null && existingItem.ItemType == CartItemType.MAIN_ITEM)
            {
                // Cập nhật bể hiện có
                existingItem.Quantity = existingItem.TerrariumVariantQuantity ?? 0;
                existingItem.TotalPrice = request.TotalPrice;
                existingItem.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.CartItem.UpdateAsync(existingItem);
                mainCartItem = existingItem;
            }
            else
            {
                // Tạo mới bể chính
                mainCartItem = new CartItem
                {
                    TerrariumId = request.TerrariumId,
                    CartId = cart.CartId,
                    Quantity = 1,
                    UnitPrice = 0,
                    TotalPrice = request.TotalPrice,
                    ItemType = CartItemType.MAIN_ITEM,
                    ParentCartItemId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.CartItem.CreateAsync(mainCartItem);
            }

            // Xử lý bundle accessories nếu có
            if (request.BundleAccessories != null && request.BundleAccessories.Any())
            {
                foreach (var item in request.BundleAccessories)
                {
                    // Kiểm tra phụ kiện đã tồn tại với terrarium cụ thể
                    var existingAccessory = await _unitOfWork.CartItem.GetExistingItemAsync(
                        cart.CartId,
                        request.TerrariumId, // ✅ Thêm TerrariumId để liên kết chính xác
                        item.AccessoryId
                    );

                    if (existingAccessory != null && existingAccessory.ItemType == CartItemType.BUNDLE_ACCESSORY)
                    {
                        // Cập nhật số lượng phụ kiện hiện có
                        existingAccessory.AccessoryQuantity += item.Quantity;
                        existingAccessory.Quantity = existingAccessory.AccessoryQuantity ?? 0;
                        var unitPrice = await GetUnitPriceAsync(item.AccessoryId, null);
                        existingAccessory.TotalPrice = unitPrice * existingAccessory.Quantity;
                        existingAccessory.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.CartItem.UpdateAsync(existingAccessory);
                    }
                    else
                    {
                        // Tạo mới phụ kiện bundle
                        var unitPrice = await GetUnitPriceAsync(item.AccessoryId, null);

                        var bundleAccessory = new CartItem
                        {
                            CartId = cart.CartId,
                            TerrariumId = request.TerrariumId, // ✅ Liên kết với terrarium
                            AccessoryId = item.AccessoryId, // ✅ Thêm AccessoryId
                            AccessoryQuantity = item.Quantity, // ✅ Thêm AccessoryQuantity
                            Quantity = item.Quantity, // ✅ Sửa từ request.Quantity thành item.Quantity
                            UnitPrice = unitPrice, // ✅ Đã define unitPrice ở trên
                            TotalPrice = unitPrice * item.Quantity, // ✅ Sửa từ accessoryRequest.Quantity thành item.Quantity
                            ItemType = CartItemType.BUNDLE_ACCESSORY,
                            ParentCartItemId = mainCartItem.CartItemId, // ✅ Liên kết với item cha
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.CartItem.CreateAsync(bundleAccessory);
                    }
                }
            }

            await _unitOfWork.SaveAsync();
            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Đã thêm item vào giỏ hàng");
        }


        public async Task<IBusinessResult> AddComboToCartAsync(int userId, AddComboToCartRequest request)
        {
            try
            {
                // Validate combo exists and is active
                var combo = await _unitOfWork.Combo.GetByIdAsync(request.ComboId);
                if (combo == null || !combo.IsActive)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Combo không tồn tại hoặc đã bị vô hiệu hóa");
                }

                // Check stock
                if (combo.StockQuantity < request.Quantity)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, $"Chỉ còn {combo.StockQuantity} combo trong kho");
                }

                var cart = await GetOrCreateCartAsync(userId);

                // Check if combo already exists in cart
                var existingCartItem = cart.CartItems
                    .FirstOrDefault(ci => ci.ComboId == request.ComboId && ci.ItemType == CartItemType.COMBO);

                if (existingCartItem != null)
                {
                    var newQuantity = existingCartItem.Quantity + request.Quantity;
                    if (combo.StockQuantity < newQuantity)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Tổng số lượng vượt quá tồn kho (còn {combo.StockQuantity})");
                    }

                    existingCartItem.Quantity = newQuantity;
                    existingCartItem.TotalPrice = combo.ComboPrice * newQuantity;
                    existingCartItem.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.CartItem.UpdateAsync(existingCartItem);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ComboId = request.ComboId,
                        Quantity = request.Quantity,
                        UnitPrice = combo.ComboPrice,
                        TotalPrice = combo.ComboPrice * request.Quantity,
                        ItemType = CartItemType.COMBO,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.CartItem.CreateAsync(cartItem);
                }

                await _unitOfWork.SaveAsync();
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Đã thêm combo vào giỏ hàng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding combo to cart for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_CREATE_CODE, $"Lỗi khi thêm combo vào giỏ hàng: {ex.Message}");
            }
        }

        #endregion

        #region Update/Remove Items

        public async Task<IBusinessResult> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
        {
            try
            {
                var cartItem = await _unitOfWork.CartItem.GetByIdWithCartAsync(cartItemId);

                if (cartItem == null)
                    return new BusinessResult(Const.FAIL_READ_CODE, $"CartItem with ID {cartItemId} not found.");

                if (cartItem.Cart == null)
                    return new BusinessResult(Const.FAIL_READ_CODE, $"Cart for CartItem with ID {cartItemId} not found.");

                if (cartItem.Cart.UserId != userId)
                    return new BusinessResult(Const.FAIL_READ_CODE, "You do not have permission to update this cart item.");

                // Handle combo items
                if (cartItem.ItemType == CartItemType.COMBO)
                {
                    if (!request.Quantity.HasValue)
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, "Quantity is required for combo items");

                    return await UpdateComboQuantityAsync(userId, cartItemId, request.Quantity.Value);
                }

                var cartItemResponse = new CartItemResponse
                {
                    CartItemId = cartItem.CartItemId,
                    CartId = cartItem.CartId,
                    AccessoryId = cartItem.AccessoryId,
                    TerrariumVariantId = cartItem.TerrariumVariantId,
                    ComboId = cartItem.ComboId,
                    Item = new List<CartItemDetail>(),
                    ItemType = cartItem.ItemType,
                    CreatedAt = cartItem.CreatedAt,
                    UpdatedAt = DateTime.UtcNow
                };

                decimal totalCartPrice = 0;
                int totalCartQuantity = 0;

                // Cập nhật Accessory nếu có
                if (cartItem.AccessoryId.HasValue && request.AccessoryQuantity.HasValue && request.AccessoryQuantity.Value > 0)
                {
                    // Check stock
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                    if (accessory == null)
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, "Accessory không tồn tại");

                    if (accessory.StockQuantity < request.AccessoryQuantity.Value)
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Chỉ còn {accessory.StockQuantity} {accessory.Name} trong kho");

                    decimal unitPrice = accessory.Price;
                    cartItem.AccessoryQuantity = request.AccessoryQuantity.Value;

                    // Get first image URL from AccessoryImages collection
                    string imageUrl = accessory.AccessoryImages?.FirstOrDefault()?.ImageUrl ?? "";

                    var accessoryDetail = new CartItemDetail
                    {
                        ProductName = accessory.Name,
                        Quantity = request.AccessoryQuantity.Value,
                        Price = unitPrice,
                        TotalPrice = unitPrice * request.AccessoryQuantity.Value,
                        ImageUrl = imageUrl, // Fixed: Get first image URL
                        ProductType = "Accessory"
                    };

                    cartItemResponse.Item.Add(accessoryDetail);
                    totalCartPrice += accessoryDetail.TotalPrice;
                    totalCartQuantity += accessoryDetail.Quantity;
                }
                else if (cartItem.AccessoryId.HasValue)
                {
                    cartItem.AccessoryQuantity = 0;
                }

                // Cập nhật Terrarium nếu có
                if (cartItem.TerrariumVariantId.HasValue && request.VariantQuantity.HasValue && request.VariantQuantity.Value > 0)
                {
                    // Check stock
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
                    if (variant == null)
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, "Terrarium variant không tồn tại");

                    if (variant.StockQuantity < request.VariantQuantity.Value)
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Chỉ còn {variant.StockQuantity} {variant.VariantName} trong kho");

                    decimal unitPrice = variant.Price;
                    cartItem.TerrariumVariantQuantity = request.VariantQuantity.Value;

                    var terrariumDetail = new CartItemDetail
                    {
                        ProductName = $"Terrarium {variant.VariantName}",
                        Quantity = request.VariantQuantity.Value,
                        Price = unitPrice,
                        TotalPrice = unitPrice * request.VariantQuantity.Value,
                        ImageUrl = variant.UrlImage, // This should be string already
                        ProductType = "Terrarium"
                    };

                    cartItemResponse.Item.Add(terrariumDetail);
                    totalCartPrice += terrariumDetail.TotalPrice;
                    totalCartQuantity += terrariumDetail.Quantity;
                }
                else if (cartItem.TerrariumVariantId.HasValue)
                {
                    cartItem.TerrariumVariantQuantity = 0;
                }

                // Nếu không còn sản phẩm nào => xóa item
                if (totalCartQuantity == 0)
                {
                    return await RemoveFromCartAsync(userId, cartItemId);
                }

                // Cập nhật lại tổng giá và số lượng
                cartItem.Quantity = totalCartQuantity;
                cartItem.TotalPrice = totalCartPrice;
                cartItem.UnitPrice = totalCartQuantity > 0 ? totalCartPrice / totalCartQuantity : 0;
                cartItem.UpdatedAt = DateTime.UtcNow;

                cartItemResponse.TotalCartPrice = totalCartPrice;
                cartItemResponse.TotalCartQuantity = totalCartQuantity;
                cartItemResponse.IsInStock = true; // Already checked above
                cartItemResponse.MaxQuantity = GetMaxQuantityForItem(cartItem);

                await _unitOfWork.CartItem.UpdateAsync(cartItem);
                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, cartItemResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi cập nhật sản phẩm: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> UpdateComboQuantityAsync(int userId, int cartItemId, int quantity)
        {
            try
            {
                var cartItem = await _unitOfWork.CartItem.GetByIdWithCartAsync(cartItemId);
                if (cartItem == null || cartItem.Cart.UserId != userId)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy item trong giỏ hàng");
                }

                if (cartItem.ItemType != CartItemType.COMBO || !cartItem.ComboId.HasValue)
                {
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Item không phải là combo");
                }

                if (quantity <= 0)
                {
                    await _unitOfWork.CartItem.RemoveAsync(cartItem);
                    await _unitOfWork.SaveAsync();
                    return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Đã xóa combo khỏi giỏ hàng");
                }

                // Check stock
                var combo = await _unitOfWork.Combo.GetByIdAsync(cartItem.ComboId.Value);
                if (combo == null || !combo.IsActive)
                {
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Combo không tồn tại hoặc đã bị vô hiệu hóa");
                }

                if (combo.StockQuantity < quantity)
                {
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Chỉ còn {combo.StockQuantity} combo trong kho");
                }

                cartItem.Quantity = quantity;
                cartItem.TotalPrice = combo.ComboPrice * quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.CartItem.UpdateAsync(cartItem);
                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật số lượng combo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating combo quantity for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi cập nhật số lượng combo: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> RemoveFromCartAsync(int userId, int itemId)
        {
            try
            {
                var cartItem = await _unitOfWork.CartItem.GetByIdWithCartAsync(itemId);
                if (cartItem == null || cartItem.Cart.UserId != userId)
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy item trong giỏ hàng");

                // Nếu là main item, xóa cả bundle accessories
                if (cartItem.ItemType == CartItemType.MAIN_ITEM)
                {
                    var bundleAccessories = await _unitOfWork.CartItem.GetBundleAccessoriesByParentIdAsync(itemId);
                    foreach (var accessory in bundleAccessories)
                    {
                        await _unitOfWork.CartItem.RemoveAsync(accessory);
                    }
                }

                await _unitOfWork.CartItem.RemoveAsync(cartItem);
                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Đã xóa item khỏi giỏ hàng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_DELETE_CODE, $"Lỗi khi xóa item khỏi giỏ hàng: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> ClearCartAsync(int userId)
        {
            try
            {
                var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
                if (cart == null)
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy giỏ hàng");

                var itemCount = cart.CartItems.Count;
                await _unitOfWork.Cart.ClearAsync(userId);
                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_DELETE_CODE, $"Đã xóa {itemCount} item khỏi giỏ hàng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_DELETE_CODE, $"Lỗi khi xóa giỏ hàng: {ex.Message}");
            }
        }

        #endregion

        #region Checkout

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
                    // Handle combo items
                    if (cartItem.ItemType == CartItemType.COMBO && cartItem.ComboId.HasValue)
                    {
                        var combo = await _unitOfWork.Combo.GetByIdAsync(cartItem.ComboId.Value);
                        if (combo != null)
                        {
                            order.OrderItems.Add(new OrderItem
                            {
                                ComboId = cartItem.ComboId,
                                Quantity = cartItem.Quantity,
                                UnitPrice = combo.ComboPrice,
                                TotalPrice = combo.ComboPrice * cartItem.Quantity,
                                ItemType = "Combo"
                            });

                            totalCartPrice += combo.ComboPrice * cartItem.Quantity;

                            // Update combo stock and sold quantity
                            combo.StockQuantity -= cartItem.Quantity;
                            combo.SoldQuantity += cartItem.Quantity;
                            await _unitOfWork.Combo.UpdateAsync(combo);
                        }
                    }

                    // Handle accessory items
                    if (cartItem.AccessoryId.HasValue && cartItem.AccessoryQuantity.HasValue)
                    {
                        var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                        if (accessory != null)
                        {
                            var quantity = cartItem.AccessoryQuantity.Value;
                            var unitPrice = accessory.Price;
                            var totalPrice = unitPrice * quantity;

                            order.OrderItems.Add(new OrderItem
                            {
                                AccessoryId = cartItem.AccessoryId,
                                AccessoryQuantity = quantity,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                TotalPrice = totalPrice,
                                ItemType = "Single"
                            });

                            totalCartPrice += totalPrice;

                            // Update accessory stock
                            accessory.StockQuantity -= quantity;
                            await _unitOfWork.Accessory.UpdateAsync(accessory);
                        }
                    }

                    // Handle terrarium variant items
                    if (cartItem.TerrariumVariantId.HasValue && cartItem.TerrariumVariantQuantity.HasValue)
                    {
                        var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
                        if (variant != null)
                        {
                            var quantity = cartItem.TerrariumVariantQuantity.Value;
                            var unitPrice = variant.Price;
                            var totalPrice = unitPrice * quantity;

                            order.OrderItems.Add(new OrderItem
                            {
                                TerrariumVariantId = cartItem.TerrariumVariantId,
                                TerrariumVariantQuantity = quantity,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                TotalPrice = totalPrice,
                                ItemType = "Single"
                            });

                            totalCartPrice += totalPrice;

                            // Update variant stock
                            variant.StockQuantity -= quantity;
                            await _unitOfWork.TerrariumVariant.UpdateAsync(variant);
                        }
                    }
                }

                order.TotalAmount = totalCartPrice;
                await _unitOfWork.Order.CreateAsync(order);

                // Clear cart
                foreach (var cartItem in cart.CartItems.ToList())
                {
                    await _unitOfWork.CartItem.RemoveAsync(cartItem);
                }

                await _unitOfWork.SaveAsync();

                var orderResponse = _mapper.Map<OrderResponse>(order);
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Checkout thành công", orderResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for user {UserId}", userId);
                return new BusinessResult(Const.ERROR_EXCEPTION, $"Lỗi khi checkout: {ex.Message}");
            }
        }
        public async Task<IBusinessResult> CheckoutItemAsync(int userId, int cartItemId)
        {
            try
            {
                var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
                if (cart == null || !cart.CartItems.Any())
                    return new BusinessResult(Const.FAIL_READ_CODE, "Giỏ hàng trống hoặc không tồn tại.");

                var defaultAddress = await _unitOfWork.Address.GetDefaultByUserIdAsync(userId);
                if (defaultAddress == null)
                    return new BusinessResult(Const.BAD_REQUEST_CODE, "Vui lòng thêm địa chỉ trước khi thanh toán.");

                var cartItem = cart.CartItems.FirstOrDefault(c => c.CartItemId == cartItemId);
                if (cartItem == null)
                    return new BusinessResult(Const.BAD_REQUEST_CODE, "Sản phẩm không tồn tại trong giỏ.");

                var order = new Order
                {
                    UserId = userId,
                    AddressId = defaultAddress.AddressId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatusEnum.Pending,
                    PaymentStatus = "Unpaid",
                    OrderItems = new List<OrderItem>()
                };

                decimal totalPrice = 0;

                // === Xử lý combo ===
                if (cartItem.ItemType == CartItemType.COMBO && cartItem.ComboId.HasValue)
                {
                    var combo = await _unitOfWork.Combo.GetByIdAsync(cartItem.ComboId.Value);
                    if (combo != null)
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            ComboId = cartItem.ComboId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = combo.ComboPrice,
                            TotalPrice = combo.ComboPrice * cartItem.Quantity,
                            ItemType = "Combo"
                        });

                        totalPrice += combo.ComboPrice * cartItem.Quantity;

                        combo.StockQuantity -= cartItem.Quantity;
                        combo.SoldQuantity += cartItem.Quantity;
                        await _unitOfWork.Combo.UpdateAsync(combo);
                    }
                }

                // === Xử lý accessory ===
                if (cartItem.AccessoryId.HasValue && cartItem.AccessoryQuantity.HasValue)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                    if (accessory != null)
                    {
                        var qty = cartItem.AccessoryQuantity.Value;
                        var unitPrice = accessory.Price;
                        var subTotal = qty * unitPrice;

                        order.OrderItems.Add(new OrderItem
                        {
                            AccessoryId = cartItem.AccessoryId,
                            Quantity = qty,
                            UnitPrice = unitPrice,
                            TotalPrice = subTotal,
                            ItemType = "Single"
                        });

                        totalPrice += subTotal;

                        accessory.StockQuantity -= qty;
                        await _unitOfWork.Accessory.UpdateAsync(accessory);
                    }
                }

                // === Xử lý terrarium variant ===
                if (cartItem.TerrariumVariantId.HasValue && cartItem.TerrariumVariantQuantity.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
                    if (variant != null)
                    {
                        var qty = cartItem.TerrariumVariantQuantity.Value;
                        var unitPrice = variant.Price;
                        var subTotal = qty * unitPrice;

                        order.OrderItems.Add(new OrderItem
                        {
                            TerrariumVariantId = cartItem.TerrariumVariantId,
                            Quantity = qty,
                            UnitPrice = unitPrice,
                            TotalPrice = subTotal,
                            ItemType = "Single"
                        });

                        totalPrice += subTotal;

                        variant.StockQuantity -= qty;
                        await _unitOfWork.TerrariumVariant.UpdateAsync(variant);
                    }
                }

                order.TotalAmount = totalPrice;
                await _unitOfWork.Order.CreateAsync(order);

                // Xoá item vừa checkout khỏi giỏ
                await _unitOfWork.CartItem.RemoveAsync(cartItem);

                await _unitOfWork.SaveAsync();

                var orderResponse = _mapper.Map<OrderResponse>(order);
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Checkout sản phẩm thành công", orderResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout item {CartItemId} for user {UserId}", cartItemId, userId);
                return new BusinessResult(Const.ERROR_EXCEPTION, $"Lỗi khi checkout: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> CheckoutMultipleItemsAsync(int userId, List<int> cartItemIds)
        {
            try
            {
                if (cartItemIds == null || cartItemIds.Count == 0)
                    return new BusinessResult(Const.BAD_REQUEST_CODE, "Vui lòng chọn sản phẩm để thanh toán.");

                // Lấy giỏ hàng của user
                var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
                if (cart == null || !cart.CartItems.Any())
                    return new BusinessResult(Const.FAIL_READ_CODE, "Giỏ hàng trống hoặc không tồn tại.");

                // Lọc ra các item được chọn
                var selectedIds = cartItemIds.Distinct().ToHashSet();
                var itemsToCheckout = cart.CartItems
                    .Where(ci => selectedIds.Contains(ci.CartItemId))
                    .ToList();

                if (itemsToCheckout.Count == 0)
                    return new BusinessResult(Const.BAD_REQUEST_CODE, "Không tìm thấy sản phẩm hợp lệ trong giỏ.");

                // Địa chỉ mặc định
                var defaultAddress = await _unitOfWork.Address.GetDefaultByUserIdAsync(userId);
                if (defaultAddress == null)
                    return new BusinessResult(Const.BAD_REQUEST_CODE, "Vui lòng thêm địa chỉ trước khi thanh toán.");

                // Tạo đơn
                var order = new Order
                {
                    UserId = userId,
                    AddressId = defaultAddress.AddressId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatusEnum.Pending,
                    PaymentStatus = "Unpaid",
                    OrderItems = new List<OrderItem>()
                };

                decimal totalCartPrice = 0m;

                // (Tuỳ bạn: nếu UnitOfWork hỗ trợ transaction, nên bọc trong transaction)
                foreach (var cartItem in itemsToCheckout)
                {
                    // === COMBO ===
                    if (cartItem.ItemType == CartItemType.COMBO && cartItem.ComboId.HasValue)
                    {
                        var combo = await _unitOfWork.Combo.GetByIdAsync(cartItem.ComboId.Value);
                        if (combo == null)
                            return new BusinessResult(Const.BAD_REQUEST_CODE, "Combo không tồn tại.");

                        // kiểm tồn (nếu dùng)
                        if (combo.StockQuantity < cartItem.Quantity)
                            return new BusinessResult(Const.BAD_REQUEST_CODE, $"Combo không đủ tồn kho (cần {cartItem.Quantity}, còn {combo.StockQuantity}).");

                        var lineTotal = combo.ComboPrice * cartItem.Quantity;

                        order.OrderItems.Add(new OrderItem
                        {
                            ComboId = cartItem.ComboId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = combo.ComboPrice,
                            TotalPrice = lineTotal,
                            ItemType = "Combo"
                        });

                        totalCartPrice += lineTotal;

                        combo.StockQuantity -= cartItem.Quantity;
                        combo.SoldQuantity += cartItem.Quantity;
                        await _unitOfWork.Combo.UpdateAsync(combo);
                    }

                    // === ACCESSORY ===
                    if (cartItem.AccessoryId.HasValue && cartItem.AccessoryQuantity.HasValue)
                    {
                        var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                        if (accessory == null)
                            return new BusinessResult(Const.BAD_REQUEST_CODE, "Phụ kiện không tồn tại.");

                        var qty = cartItem.AccessoryQuantity.Value;

                        if (qty <= 0)
                            return new BusinessResult(Const.BAD_REQUEST_CODE, "Số lượng phụ kiện không hợp lệ.");

                        if (accessory.StockQuantity < qty)
                            return new BusinessResult(Const.BAD_REQUEST_CODE, $"Phụ kiện '{accessory.Name}' không đủ tồn kho (cần {qty}, còn {accessory.StockQuantity}).");

                        var unitPrice = accessory.Price;
                        var lineTotal = unitPrice * qty;

                        order.OrderItems.Add(new OrderItem
                        {
                            AccessoryId = cartItem.AccessoryId,
                            AccessoryQuantity = qty,
                            Quantity = qty,
                            UnitPrice = unitPrice,
                            TotalPrice = lineTotal,
                            ItemType = "Single"
                        });

                        totalCartPrice += lineTotal;

                        accessory.StockQuantity -= qty;
                        await _unitOfWork.Accessory.UpdateAsync(accessory);
                    }

                    // === TERRARIUM VARIANT ===
                    if (cartItem.TerrariumVariantId.HasValue && cartItem.TerrariumVariantQuantity.HasValue)
                    {
                        var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(cartItem.TerrariumVariantId.Value);
                        if (variant == null)
                            return new BusinessResult(Const.BAD_REQUEST_CODE, "Biến thể terrarium không tồn tại.");

                        var qty = cartItem.TerrariumVariantQuantity.Value;

                        if (qty <= 0)
                            return new BusinessResult(Const.BAD_REQUEST_CODE, "Số lượng biến thể không hợp lệ.");

                        if (variant.StockQuantity < qty)
                            return new BusinessResult(Const.BAD_REQUEST_CODE, $"Biến thể '{variant.VariantName}' không đủ tồn kho (cần {qty}, còn {variant.StockQuantity}).");

                        var unitPrice = variant.Price;
                        var lineTotal = unitPrice * qty;

                        order.OrderItems.Add(new OrderItem
                        {
                            TerrariumVariantId = cartItem.TerrariumVariantId,
                            TerrariumVariantQuantity = qty,
                            Quantity = qty,
                            UnitPrice = unitPrice,
                            TotalPrice = lineTotal,
                            ItemType = "Single"
                        });

                        totalCartPrice += lineTotal;

                        variant.StockQuantity -= qty;
                        await _unitOfWork.TerrariumVariant.UpdateAsync(variant);
                    }
                }

                // Hoàn tất đơn
                order.TotalAmount = totalCartPrice;
                await _unitOfWork.Order.CreateAsync(order);

                // Xoá các item đã checkout khỏi giỏ
                foreach (var item in itemsToCheckout)
                    await _unitOfWork.CartItem.RemoveAsync(item);

                await _unitOfWork.SaveAsync();

                var orderResponse = _mapper.Map<OrderResponse>(order);
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Checkout thành công các sản phẩm đã chọn.", orderResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout multiple items for user {UserId}", userId);
                return new BusinessResult(Const.ERROR_EXCEPTION, $"Lỗi khi checkout: {ex.Message}");
            }
        }

        #endregion

        #region Private Helper Methods

        //private async Task<CartItem> AddTerrariumToCartAsync(Cart cart, AddCartItemRequest request)
        //{
        //    // Kiểm tra đã có bể này trong giỏ chưa
        //    var existingItem = await _unitOfWork.CartItem.GetExistingTerrariumItemAsync(
        //        cart.CartId,
        //        request.TerrariumId
        //    );

        //    if (existingItem != null && existingItem.ItemType == CartItemType.MAIN_ITEM)
        //    {
        //        // Cập nhật số lượng bể hiện có
        //        existingItem.TerrariumVariantQuantity += request.VariantQuantity ?? 1;
        //        existingItem.Quantity = existingItem.TerrariumVariantQuantity ?? 0;

        //        var unitPrice = await GetUnitPriceAsync(null, existingItem.TerrariumVariantId);
        //        existingItem.TotalPrice = unitPrice * existingItem.Quantity;
        //        existingItem.UpdatedAt = DateTime.UtcNow;

        //        await _unitOfWork.CartItem.UpdateAsync(existingItem);
        //        return existingItem;
        //    }

        //    // Tạo mới bể
        //    var quantity = request.VariantQuantity ?? 1;
        //    foreach(var item in request.TerrariumVariants)
        //    {
        //        _unitOfWork.TerrariumVariant.Create(new TerrariumVariant
        //        {
        //            TerrariumId = request.TerrariumId,
        //            VariantName = item.VariantName,
        //            Price = item.Price,
        //            StockQuantity = item.StockQuantity,
        //            UrlImage = item.UrlImage,
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow
        //        });
        //    }
        //    var terrariumVariant = await _unitOfWork.TerrariumVariant.GetAllByTerrariumIdAsync(request.TerrariumId);
        //    foreach (var variant in terrariumVariant)
        //    {
        //        var cartItem = new CartItem
        //        {
        //            TerrariumId = request.TerrariumId,
        //            CartId = cart.CartId,
        //            TerrariumVariantId = variant.TerrariumVariantId,
        //            TerrariumVariantQuantity = quantity,
        //            Quantity = quantity,
        //            UnitPrice = variant.Price,
        //            TotalPrice = variant.Price * quantity,
        //            ItemType = CartItemType.MAIN_ITEM,
        //            ParentCartItemId = null,
        //            CreatedAt = DateTime.UtcNow,
        //            UpdatedAt = DateTime.UtcNow
        //        };
        //        await _unitOfWork.CartItem.CreateAsync(cartItem);
        //        return cartItem;
        //    }
           

        //}

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
                ItemType = CartItemType.SINGLE,
                ParentCartItemId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CartItem.CreateAsync(cartItem);
            return cartItem;
        }

        private async Task<CartItemResponse> BuildCartItemResponseAsync(CartItem cartItem)
        {
            // ✅ Get all data concurrently
            var terrariumImagesTask = cartItem.TerrariumId > 0
                ? _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(cartItem.TerrariumId)
                : Task.FromResult<IEnumerable<TerrariumImage>>(new List<TerrariumImage>());

            var terrariumVariantsTask = cartItem.TerrariumId > 0
                ? _unitOfWork.TerrariumVariant.GetAllByTerrariumIdAsync(cartItem.TerrariumId)
                : Task.FromResult<IEnumerable<TerrariumVariant>>(new List<TerrariumVariant>());

            // ✅ Get accessory images if accessory exists
            var accessoryImagesTask = cartItem.AccessoryId.HasValue
                ? _unitOfWork.AccessoryImage.GetAllByAccessoryIdAsync(cartItem.AccessoryId.Value)
                : Task.FromResult<IEnumerable<AccessoryImage>>(new List<AccessoryImage>());

            await Task.WhenAll(terrariumImagesTask, terrariumVariantsTask, accessoryImagesTask);

            var terrariumImages = await terrariumImagesTask;
            var terrariumVariants = await terrariumVariantsTask;
            var accessoryImages = await accessoryImagesTask;

            // ✅ Build variants list properly
            var variants = new List<TerrariumVariantResponse>();
            if (terrariumVariants?.Any() == true)
            {
                variants = terrariumVariants.Select(itemVariant => new TerrariumVariantResponse
                {
                    TerrariumVariantId = itemVariant.TerrariumVariantId,
                    TerrariumId = itemVariant.TerrariumId,
                    VariantName = itemVariant.VariantName,
                    Price = itemVariant.Price,
                    StockQuantity = itemVariant.StockQuantity,
                    UrlImage = itemVariant.UrlImage,
                    CreatedAt = itemVariant.CreatedAt,
                    UpdatedAt = itemVariant.UpdatedAt
                }).ToList();
            }

            var itemDetails = new List<CartItemDetail>();
            decimal totalPrice = 0m;
            int totalQuantity = 0;

            // ✅ Build detail cho Terrarium với ALL images
            if (cartItem.TerrariumId > 0)
            {
                var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(cartItem.TerrariumId);
                if (terrarium != null)
                {
                    var qty = cartItem.TerrariumVariantQuantity ?? cartItem.Quantity;
                    var price = cartItem.UnitPrice;

                    var terrariumDetail = new CartItemDetail
                    {
                        ProductName = terrarium.TerrariumName,
                        Quantity = qty,
                        Price = price,
                        TotalPrice = price * qty,
                        ImageUrl = terrariumImages?.FirstOrDefault()?.ImageUrl, // Primary image
                        ImageUrls = terrariumImages?.Select(img => img.ImageUrl).ToList() ?? new List<string>(), // ✅ ALL images
                        ProductType = "Terrarium",
                        TerrariumVariants = variants
                    };
                    itemDetails.Add(terrariumDetail);

                    totalPrice += price * qty;
                    totalQuantity += qty;
                }
            }

            // ✅ Build detail cho Accessory với ALL accessory images
            if (cartItem.AccessoryId.HasValue && (cartItem.AccessoryQuantity ?? 0) > 0)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(cartItem.AccessoryId.Value);
                if (accessory != null)
                {
                    var qty = cartItem.AccessoryQuantity!.Value;
                    var price = accessory.Price;

                    itemDetails.Add(new CartItemDetail
                    {
                        ProductName = accessory.Name,
                        Quantity = qty,
                        Price = price,
                        TotalPrice = price * qty,
                        ImageUrl = accessoryImages?.FirstOrDefault()?.ImageUrl, // ✅ Primary image from AccessoryImages
                        ImageUrls = accessoryImages?.Select(img => img.ImageUrl).ToList() ?? new List<string>(), // ✅ ALL accessory images
                        ProductType = "Accessory"
                    });

                    totalPrice += price * qty;
                    totalQuantity += qty;
                }
            }

            var response = new CartItemResponse
            {
                TerrariumId = cartItem.TerrariumId,
                CartItemId = cartItem.CartItemId,
                CartId = cartItem.CartId,
                AccessoryId = cartItem.AccessoryId,
                TerrariumVariantId = cartItem.TerrariumVariantId,
                //Item = itemDetails,
                //TotalCartPrice = totalPrice,
                //TotalCartQuantity = totalQuantity,
                TotalCartPrice = cartItem.TotalPrice,
                TotalCartQuantity = cartItem.Quantity,
                ItemType = cartItem.ItemType,
                CreatedAt = cartItem.CreatedAt,
                UpdatedAt = cartItem.UpdatedAt
            };
            response.TotalCartPrice = totalPrice;
            response.TotalCartQuantity = totalQuantity;
            return response;

        }

        public async Task<IBusinessResult> ValidateCartAsync(int userId)
        {
            try
            {
                var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return new BusinessResult(Const.SUCCESS_READ_CODE, "Giỏ hàng trống", new
                    {
                        IsValid = true,
                        Issues = new List<string>(),
                        TotalItems = 0,
                        Warnings = new List<string>()
                    });
                }

                var cartItems = cart.CartItems.ToList();
                if (!cartItems.Any())
                {
                    return new BusinessResult(Const.SUCCESS_READ_CODE, "Giỏ hàng trống", new
                    {
                        IsValid = true,
                        Issues = new List<string>(),
                        TotalItems = 0,
                        Warnings = new List<string>()
                    });
                }

                var issues = new List<string>();
                var warnings = new List<string>();
                var outOfStockItems = new List<object>();
                var discontinuedItems = new List<object>();
                var priceChangedItems = new List<object>();

                foreach (var item in cartItems)
                {
                    // Validate Combo Items
                    if (item.ItemType == CartItemType.COMBO && item.ComboId.HasValue)
                    {
                        var combo = await _unitOfWork.Combo.GetByIdAsync(item.ComboId.Value);
                        if (combo == null)
                        {
                            issues.Add($"Combo với ID {item.ComboId} không tồn tại");
                            discontinuedItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Combo",
                                Name = "Unknown Combo",
                                Issue = "Không tồn tại"
                            });
                            continue;
                        }

                        if (!combo.IsActive)
                        {
                            issues.Add($"Combo '{combo.Name}' đã bị vô hiệu hóa");
                            discontinuedItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Combo",
                                Name = combo.Name,
                                Issue = "Đã bị vô hiệu hóa"
                            });
                            continue;
                        }

                        if (combo.StockQuantity <= 0)
                        {
                            issues.Add($"Combo '{combo.Name}' đã hết hàng");
                            outOfStockItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Combo",
                                Name = combo.Name,
                                RequestedQuantity = item.Quantity,
                                AvailableQuantity = 0
                            });
                        }
                        else if (combo.StockQuantity < item.Quantity)
                        {
                            issues.Add($"Combo '{combo.Name}' chỉ còn {combo.StockQuantity} trong kho (bạn chọn {item.Quantity})");
                            outOfStockItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Combo",
                                Name = combo.Name,
                                RequestedQuantity = item.Quantity,
                                AvailableQuantity = combo.StockQuantity
                            });
                        }

                        // Check if price changed (nếu bạn lưu giá tại thời điểm add to cart)
                        if (item.UnitPrice != combo.ComboPrice)
                        {
                            warnings.Add($"Giá combo '{combo.Name}' đã thay đổi từ {item.UnitPrice:C} thành {combo.ComboPrice:C}");
                            priceChangedItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Combo",
                                Name = combo.Name,
                                OldPrice = item.UnitPrice,
                                NewPrice = combo.ComboPrice,
                                Difference = combo.ComboPrice - item.UnitPrice
                            });
                        }

                        // Validate combo items availability
                        var comboItems = await _unitOfWork.ComboItem.GetItemsByComboIdAsync(combo.ComboId);
                        foreach (var comboItem in comboItems)
                        {
                            if (comboItem.TerrariumVariantId.HasValue)
                            {
                                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(comboItem.TerrariumVariantId.Value);
                                if (variant == null || variant.StockQuantity < (comboItem.Quantity * item.Quantity))
                                {
                                    issues.Add($"Sản phẩm '{variant?.VariantName ?? "Unknown"}' trong combo '{combo.Name}' không đủ hàng");
                                }
                            }
                            else if (comboItem.AccessoryId.HasValue)
                            {
                                var accessory = await _unitOfWork.Accessory.GetByIdAsync(comboItem.AccessoryId.Value);
                                if (accessory == null || accessory.StockQuantity < (comboItem.Quantity * item.Quantity))
                                {
                                    issues.Add($"Phụ kiện '{accessory?.Name ?? "Unknown"}' trong combo '{combo.Name}' không đủ hàng");
                                }
                            }
                        }
                    }

                    // Validate Single Accessory Items
                    if (item.AccessoryId.HasValue && item.AccessoryQuantity.HasValue)
                    {
                        var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                        if (accessory == null)
                        {
                            issues.Add($"Phụ kiện với ID {item.AccessoryId} không tồn tại");
                            discontinuedItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Accessory",
                                Name = "Unknown Accessory",
                                Issue = "Không tồn tại"
                            });
                            continue;
                        }


                        if (accessory.StockQuantity <= 0)
                        {
                            issues.Add($"Phụ kiện '{accessory.Name}' đã hết hàng");
                            outOfStockItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Accessory",
                                Name = accessory.Name,
                                RequestedQuantity = item.AccessoryQuantity,
                                AvailableQuantity = 0
                            });
                        }
                        else if (accessory.StockQuantity < item.AccessoryQuantity)
                        {
                            issues.Add($"Phụ kiện '{accessory.Name}' chỉ còn {accessory.StockQuantity} trong kho (bạn chọn {item.AccessoryQuantity})");
                            outOfStockItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Accessory",
                                Name = accessory.Name,
                                RequestedQuantity = item.AccessoryQuantity,
                                AvailableQuantity = accessory.StockQuantity
                            });
                        }

                        // Check price change
                        if (Math.Abs(item.UnitPrice - accessory.Price) > 0.01m)
                        {
                            warnings.Add($"Giá phụ kiện '{accessory.Name}' đã thay đổi từ {item.UnitPrice:C} thành {accessory.Price:C}");
                            priceChangedItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "Accessory",
                                Name = accessory.Name,
                                OldPrice = item.UnitPrice,
                                NewPrice = accessory.Price,
                                Difference = accessory.Price - item.UnitPrice
                            });
                        }
                    }

                    // Validate Terrarium Variant Items
                    if (item.TerrariumVariantId.HasValue && item.TerrariumVariantQuantity.HasValue)
                    {
                        var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                        if (variant == null)
                        {
                            issues.Add($"Biến thể terrarium với ID {item.TerrariumVariantId} không tồn tại");
                            discontinuedItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "TerrariumVariant",
                                Name = "Unknown Variant",
                                Issue = "Không tồn tại"
                            });
                            continue;
                        }

                        if (variant.StockQuantity <= 0)
                        {
                            issues.Add($"Biến thể '{variant.VariantName}' đã hết hàng");
                            outOfStockItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "TerrariumVariant",
                                Name = variant.VariantName,
                                RequestedQuantity = item.TerrariumVariantQuantity,
                                AvailableQuantity = 0
                            });
                        }
                        else if (variant.StockQuantity < item.TerrariumVariantQuantity)
                        {
                            issues.Add($"Biến thể '{variant.VariantName}' chỉ còn {variant.StockQuantity} trong kho (bạn chọn {item.TerrariumVariantQuantity})");
                            outOfStockItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "TerrariumVariant",
                                Name = variant.VariantName,
                                RequestedQuantity = item.TerrariumVariantQuantity,
                                AvailableQuantity = variant.StockQuantity
                            });
                        }

                        // Check price change
                        if (Math.Abs(item.UnitPrice - variant.Price) > 0.01m)
                        {
                            warnings.Add($"Giá biến thể '{variant.VariantName}' đã thay đổi từ {item.UnitPrice:C} thành {variant.Price:C}");
                            priceChangedItems.Add(new
                            {
                                CartItemId = item.CartItemId,
                                Type = "TerrariumVariant",
                                Name = variant.VariantName,
                                OldPrice = item.UnitPrice,
                                NewPrice = variant.Price,
                                Difference = variant.Price - item.UnitPrice
                            });
                        }
                    }
                }

                // Calculate totals
                var totalOriginalAmount = cartItems.Sum(item => item.TotalPrice);
                var totalCurrentAmount = await CalculateCurrentCartTotal(cartItems);
                var totalSavings = totalOriginalAmount - totalCurrentAmount;

                var validationResult = new
                {
                    IsValid = issues.Count == 0,
                    HasWarnings = warnings.Count > 0,
                    TotalItems = cartItems.Count,
                    TotalQuantity = cartItems.Sum(c => c.Quantity),

                    // Issues and Warnings
                    Issues = issues,
                    Warnings = warnings,

                    // Detailed breakdown
                    OutOfStockItems = outOfStockItems,
                    DiscontinuedItems = discontinuedItems,
                    PriceChangedItems = priceChangedItems,

                    // Financial summary
                    TotalOriginalAmount = totalOriginalAmount,
                    TotalCurrentAmount = totalCurrentAmount,
                    TotalSavings = totalSavings,

                    // Recommendations
                    Recommendations = GenerateRecommendations(issues, warnings, outOfStockItems, priceChangedItems),

                    // Validation timestamp
                    ValidatedAt = DateTime.UtcNow
                };

                var message = issues.Count == 0 ?
                    (warnings.Count > 0 ? "Giỏ hàng hợp lệ nhưng có một số thay đổi" : "Giỏ hàng hợp lệ") :
                    "Giỏ hàng có vấn đề cần xử lý";

                return new BusinessResult(Const.SUCCESS_READ_CODE, message, validationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi validate giỏ hàng: {ex.Message}");
            }
        }

        // Helper method to calculate current cart total with updated prices
        private async Task<decimal> CalculateCurrentCartTotal(List<CartItem> cartItems)
        {
            decimal total = 0;

            foreach (var item in cartItems)
            {
                if (item.ItemType == CartItemType.COMBO && item.ComboId.HasValue)
                {
                    var combo = await _unitOfWork.Combo.GetByIdAsync(item.ComboId.Value);
                    if (combo != null && combo.IsActive)
                    {
                        total += combo.ComboPrice * item.Quantity;
                    }
                }
                else if (item.AccessoryId.HasValue && item.AccessoryQuantity.HasValue)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                    if (accessory != null)
                    {
                        total += accessory.Price * item.AccessoryQuantity.Value;
                    }
                }
                else if (item.TerrariumVariantId.HasValue && item.TerrariumVariantQuantity.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                    if (variant != null)
                    {
                        total += variant.Price * item.TerrariumVariantQuantity.Value;
                    }
                }
            }

            return total;
        }

        // Helper method to generate recommendations based on validation results
        private List<string> GenerateRecommendations(
            List<string> issues,
            List<string> warnings,
            List<object> outOfStockItems,
            List<object> priceChangedItems)
        {
            var recommendations = new List<string>();

            if (outOfStockItems.Any())
            {
                recommendations.Add("Cập nhật số lượng hoặc xóa các sản phẩm hết hàng khỏi giỏ hàng");
                recommendations.Add("Liên hệ với chúng tôi để được thông báo khi hàng về");
            }


            if (issues.Any())
            {
                recommendations.Add("Xử lý các vấn đề trong giỏ hàng trước khi tiến hành thanh toán");
            }

            if (!issues.Any() && !warnings.Any())
            {
                recommendations.Add("Giỏ hàng đã sẵn sàng để thanh toán");
            }

            return recommendations;
        }
        public async Task<IBusinessResult> GetCartSummaryAsync(int userId)
        {
            try
            {
                var cart = await _unitOfWork.Cart.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return new BusinessResult(Const.SUCCESS_READ_CODE, "Giỏ hàng trống", new
                    {
                        TotalItems = 0,
                        TotalQuantity = 0,
                        TotalAmount = 0m,
                        ItemTypes = new
                        {
                            ComboItems = 0,
                            SingleItems = 0,
                            BundleItems = 0
                        }
                    });
                }

                var cartItems = cart.CartItems.ToList();
                if (!cartItems.Any())
                {
                    return new BusinessResult(Const.SUCCESS_READ_CODE, "Giỏ hàng trống", new
                    {
                        TotalItems = 0,
                        TotalQuantity = 0,
                        TotalAmount = 0m,
                        ItemTypes = new
                        {
                            ComboItems = 0,
                            SingleItems = 0,
                            BundleItems = 0
                        }
                    });
                }

                var totalQuantity = cartItems.Sum(c => c.Quantity);
                decimal totalAmount = 0;

                // Calculate total amount with current prices
                foreach (var item in cartItems)
                {
                    if (item.ItemType == CartItemType.COMBO && item.ComboId.HasValue)
                    {
                        var combo = await _unitOfWork.Combo.GetByIdAsync(item.ComboId.Value);
                        if (combo != null && combo.IsActive)
                        {
                            totalAmount += combo.ComboPrice * item.Quantity;
                        }
                    }
                    else if (item.AccessoryId.HasValue && item.AccessoryQuantity.HasValue)
                    {
                        var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                        if (accessory != null)
                        {
                            totalAmount += accessory.Price * item.AccessoryQuantity.Value;
                        }
                    }
                    else if (item.TerrariumVariantId.HasValue && item.TerrariumVariantQuantity.HasValue)
                    {
                        var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                        if (variant != null)
                        {
                            totalAmount += variant.Price * item.TerrariumVariantQuantity.Value;
                        }
                    }
                }

                var summary = new
                {
                    TotalItems = cartItems.Count,
                    TotalQuantity = totalQuantity,
                    TotalAmount = totalAmount,
                    ItemTypes = new
                    {
                        ComboItems = cartItems.Count(c => c.ItemType == CartItemType.COMBO),
                        SingleItems = cartItems.Count(c => c.ItemType == CartItemType.SINGLE),
                        BundleItems = cartItems.Count(c => c.ItemType == CartItemType.MAIN_ITEM),
                        BundleAccessories = cartItems.Count(c => c.ItemType == CartItemType.BUNDLE_ACCESSORY)
                    },
                    LastUpdated = cartItems.Max(c => c.UpdatedAt)
                };

                return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy tóm tắt giỏ hàng thành công", summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary for user {UserId}", userId);
                return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy tóm tắt giỏ hàng: {ex.Message}");
            }
        }

        private async Task<CartItemResponse> BuildComboCartItemResponseAsync(CartItem cartItem)
        {
            var response = new CartItemResponse
            {
                CartItemId = cartItem.CartItemId,
                CartId = cartItem.CartId,
                ComboId = cartItem.ComboId,
                Item = new List<CartItemDetail>(),
                CreatedAt = cartItem.CreatedAt,
                UpdatedAt = cartItem.UpdatedAt
            };

            if (cartItem.ComboId.HasValue)
            {
                var combo = await _unitOfWork.Combo.GetByIdAsync(cartItem.ComboId.Value);
                if (combo != null)
                {
                    response.Item.Add(new CartItemDetail
                    {
                        ProductName = $"Combo: {combo.Name}",
                        Quantity = cartItem.Quantity,
                        Price = combo.ComboPrice,
                        TotalPrice = combo.ComboPrice * cartItem.Quantity
                    });

                    response.TotalCartPrice = combo.ComboPrice * cartItem.Quantity;
                    response.TotalCartQuantity = cartItem.Quantity;
                }
            }

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

        #endregion
        private int GetMaxQuantityForItem(CartItem cartItem)
        {
            if (cartItem.ComboId.HasValue && cartItem.Combo != null)
                return cartItem.Combo.StockQuantity;

            if (cartItem.AccessoryId.HasValue && cartItem.Accessory != null)
                return cartItem.Accessory.StockQuantity ?? 0;

            if (cartItem.TerrariumVariantId.HasValue && cartItem.TerrariumVariant != null)
                return cartItem.TerrariumVariant.StockQuantity;

            return 0;
        }
    }
}
