using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Service.IService;
using Microsoft.Extensions.Logging;

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

        return Ok(cart);
    }

    /// <summary>
    ///     Thêm nhiều món vào giỏ hàng
    /// </summary>
    [HttpPost("items/multiple")]
    [Authorize]
    public async Task<IActionResult> AddItems([FromBody] List<AddCartItemRequest> requests)
    {
        var userId = User.GetUserId();
        var items = new List<CartItem>();
        decimal totalCartPrice = 0; // Tổng giá trị của giỏ hàng
        var failedItems = new List<string>(); // Dùng để lưu các món hàng không hợp lệ

        try
        {
            foreach (var request in requests)
            {
                if (request.Quantity <= 0)
                {
                    failedItems.Add("Số lượng sản phẩm phải lớn hơn 0");
                    continue; // Bỏ qua món hàng không hợp lệ
                }

                var item = await _cartService.AddItemAsync(userId, request);
                totalCartPrice += item.TotalPrice;
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


    /// <summary>
    ///     Cập nhật số lượng món trong giỏ hàng
    /// </summary>
    [HttpPut("items/{itemId}")]
    [Authorize]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemRequest request)
    {
        var userId = User.GetUserId();

        if (request.Quantity <= 0)
            return BadRequest(new { message = "Số lượng sản phẩm phải lớn hơn 0" });

        var success = await _cartService.UpdateItemAsync(userId, itemId, request.Quantity);
        if (!success)
            return NotFound(new { message = "Item không tìm thấy trong giỏ hàng" });

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
