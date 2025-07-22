using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
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
    ///     Thêm một món vào giỏ hàng
    /// </summary>
    [HttpPost("items")]
    [Authorize]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        var userId = User.GetUserId();

        // Validate item request
        if (request.Quantity <= 0) return BadRequest(new { message = "Số lượng sản phẩm phải lớn hơn 0" });

        var item = await _cartService.AddItemAsync(userId, request);
        return CreatedAtAction(nameof(GetCart), new { itemId = item.CartItemId }, item);
    }

    /// <summary>
    ///     Cập nhật số lượng món trong giỏ hàng
    /// </summary>
    [HttpPut("items/{itemId}")]
    [Authorize]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemRequest request)
    {
        var userId = User.GetUserId();

        if (request.Quantity <= 0) return BadRequest(new { message = "Số lượng sản phẩm phải lớn hơn 0" });

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

            // Trả về thông tin đơn hàng vừa được tạo
            return CreatedAtAction(nameof(OrderController.Get), new { id = order.OrderId }, order);
        }
        catch (InvalidOperationException ex)
        {
            // Trường hợp lỗi trong quá trình checkout
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Các lỗi khác
            return StatusCode(500, new { message = "Đã có lỗi xảy ra trong quá trình thanh toán", error = ex.Message });
        }
    }
}