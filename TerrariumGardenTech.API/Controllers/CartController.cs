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
        var cart = await _cartService.GetCartAsync(userId);
        if (cart == null)
            return NotFound();

        return Ok(cart);
    }


    /// <summary>
    ///     Thêm nhiều món vào giỏ hàng
    /// </summary>
    [HttpPost("items/multiple")]
    [Authorize]
    public async Task<IActionResult> AddItems([FromBody] AddCartItemRequest request)
    {
        var userId = User.GetUserId();
        if (request == null ||
         (request.AccessoryQuantity is null && request.VariantQuantity is null) ||
         (request.AccessoryQuantity <= 0 && request.VariantQuantity <= 0))
        {
            return BadRequest(new { message = "Thông tin thêm vào giỏ hàng không hợp lệ." });
        }

        var result = await _cartService.AddItemAsync(userId, request);
        return CreatedAtAction(nameof(GetCart), new {userId}, result);
    }





    [HttpPut("items/{cartItemId}")]
    [Authorize]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        var userId = User.GetUserId();

        if (request == null ||
         (request.AccessoryQuantity is null && request.VariantQuantity is null) ||
         (request.AccessoryQuantity <= 0 && request.VariantQuantity <= 0))
        {
            return BadRequest(new { message = "Thông tin cập nhật không hợp lệ." });
        }

        var result = await _cartService.UpdateItemAsync(userId, cartItemId, request);

        if (result == null)
            return NotFound(new { message = "Không tìm thấy item trong giỏ hàng hoặc không thuộc về người dùng." });

        return Ok(result);
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
