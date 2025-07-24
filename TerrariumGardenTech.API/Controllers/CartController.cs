using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.ResponseModel.Cart;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    ///     Lấy giỏ hàng của người dùng
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.GetUserId();
        var cart = await _cartService.GetCartByUserAsync(userId);

        if (cart == null) return NotFound(new { message = "Giỏ hàng không tồn tại" });

        var cartItems = cart.CartItems.Select(item => new
        {
            item.CartItemId,
            item.CartId,
            item.AccessoryId,
            Accessory = item.Accessory?.Name,  // Thêm tên phụ kiện nếu có
            item.TerrariumVariantId,
            TerrariumVariant = item.TerrariumVariant?.VariantName,  // Thêm tên biến thể terrarium nếu có
            item.Quantity,  // Số lượng của mỗi món
            item.UnitPrice,
            item.TotalPrice,
            item.CreatedAt,
            item.UpdatedAt
        }).ToList();

        var totalCartPrice = cartItems.Sum(x => x.TotalPrice);
        return Ok(new
        {
            cart.CartId,
            cart.UserId,
            cartItems,
            totalCartPrice
        });
    }


    /// <summary>
    ///     Thêm nhiều món vào giỏ hàng
    /// </summary>
    [HttpPost("items/multiple")]
    [Authorize]
    public async Task<IActionResult> AddItems([FromBody] List<AddCartItemRequest> requests)
    {
        var userId = User.GetUserId();
        var items = new List<CartItemResponse>();  // Dùng CartItemResponse thay vì anonymous type
        decimal totalCartPrice = 0; // Tổng giá trị của giỏ hàng
        var failedItems = new List<string>(); // Dùng để lưu các món hàng không hợp lệ

        try
        {
            foreach (var request in requests)
            {
                // Kiểm tra quantity cho phụ kiện hoặc variant
                if ((request.AccessoryQuantity.HasValue && request.AccessoryQuantity.Value <= 0) &&
                    (request.VariantQuantity.HasValue && request.VariantQuantity.Value <= 0))
                {
                    failedItems.Add("Số lượng sản phẩm phải lớn hơn 0");
                    continue; // Bỏ qua món hàng không hợp lệ
                }

                // Thêm sản phẩm vào giỏ
                var item = await _cartService.AddItemAsync(userId, request);
                totalCartPrice += item.TotalPrice;

                // Thêm các thông tin sản phẩm vào danh sách
                items.Add(item);
            }

            if (failedItems.Any())
            {
                return BadRequest(new { message = "Có lỗi với một số món hàng", details = failedItems });
            }

            return CreatedAtAction(nameof(GetCart), new { userId }, new { items, totalCartPrice });
        }
        catch (Exception ex)
        {
            // Ghi log chi tiết lỗi
            _logger.LogError(ex, "Error occurred while adding items to the cart.");
            return StatusCode(500, new { message = "Đã có lỗi xảy ra trong quá trình xử lý giỏ hàng", error = ex.Message });
        }
    }





    [HttpPut("items/{cartItemId}")]
    [Authorize]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        var userId = User.GetUserId();

        // Logging userId và cartItemId
        Console.WriteLine($"Updating CartItem with ID {cartItemId} for UserID {userId}");

        // Kiểm tra số lượng phải lớn hơn 0
        if ((request.AccessoryQuantity.HasValue && request.AccessoryQuantity <= 0) &&
            (request.VariantQuantity.HasValue && request.VariantQuantity <= 0))
        {
            return BadRequest(new { message = "Số lượng sản phẩm phải lớn hơn 0" });
        }

        // Cập nhật giỏ hàng
        var success = await _cartService.UpdateItemAsync(userId, cartItemId, request.AccessoryQuantity, request.VariantQuantity);
        if (!success)
        {
            // Logging lỗi không tìm thấy mục giỏ hàng
            Console.WriteLine($"CartItem with ID {cartItemId} not found or not owned by user {userId}");
            return NotFound(new { message = "Item không tìm thấy trong giỏ hàng hoặc không thuộc quyền sở hữu của người dùng" });
        }

        // Nếu cập nhật thành công
        return NoContent();
    }




    /// <summary>
    ///     Xóa một món khỏi giỏ hàng
    /// </summary>
    [HttpDelete("items/{itemId}")]
    [Authorize]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var userId = User.GetUserId();

        var success = await _cartService.RemoveItemAsync(userId, itemId);
        if (!success)
            return NotFound(new { message = "Item không tìm thấy trong giỏ hàng" });

        return NoContent();
    }

    /// <summary>
    ///     Xóa toàn bộ giỏ hàng của người dùng
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.GetUserId();

        var success = await _cartService.ClearCartAsync(userId);
        if (!success)
            return BadRequest(new { message = "Không thể xóa giỏ hàng" });

        return NoContent();
    }

    /// <summary>
    ///     Tiến hành thanh toán cho giỏ hàng (Checkout)
    /// </summary>
    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var userId = User.GetUserId();
        try
        {
            var order = await _cartService.CheckoutAsync(userId, request);

            return CreatedAtAction(nameof(OrderController.Get), new { id = order.OrderId }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã có lỗi xảy ra trong quá trình thanh toán", error = ex.Message });
        }
    }
}
