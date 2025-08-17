using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Cart;
using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Service;

namespace TerrariumGardenTech.API.Controllers
{
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
        /// Lấy giỏ hàng của người dùng
        /// </summary>
        [HttpGet("get-all")]
        [Authorize]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _cartService.GetCartAsync(userId);

                if (result.Status == Const.SUCCESS_READ_CODE)
                    return Ok(result);

                if (result.Status == Const.FAIL_READ_CODE)
                    return NotFound(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, new { message = "Lỗi server khi lấy giỏ hàng" });
            }
        }

        /// <summary>
        /// Thêm một sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost("add-item")]
        [Authorize]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _cartService.AddItemAsync(userId, request);

                if (result.Status == Const.SUCCESS_CREATE_CODE)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, new { message = "Lỗi server khi thêm sản phẩm vào giỏ hàng" });
            }
        }

        /// <summary>
        /// Thêm combo vào giỏ hàng
        /// </summary>
        [HttpPost("add-combo")]
        [Authorize]
        public async Task<IActionResult> AddCombo([FromBody] AddComboToCartRequest request)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _cartService.AddComboToCartAsync(userId, request);

                if (result.Status == Const.SUCCESS_CREATE_CODE)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding combo to cart");
                return StatusCode(500, new { message = "Lỗi server khi thêm combo vào giỏ hàng" });
            }
        }

        /// <summary>
        /// Thêm nhiều sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost("add-items/multiple")]
        [Authorize]
        public async Task<IActionResult> AddMultipleItems([FromBody] List<AddCartItemRequest> requests)
        {
            try
            {
                var userId = User.GetUserId();

                if (requests == null || !requests.Any())
                {
                    return BadRequest(new { message = "Danh sách sản phẩm không được để trống." });
                }

                var successResults = new List<object>();
                var errors = new List<string>();

                foreach (var request in requests)
                {
                    // Validate request
                    if ((request.AccessoryQuantity is null && request.VariantQuantity is null) ||
                        (request.AccessoryQuantity <= 0 && request.VariantQuantity <= 0))
                    {
                        errors.Add("Sản phẩm phải có số lượng lớn hơn 0");
                        continue;
                    }

                    try
                    {
                        var result = await _cartService.AddItemAsync(userId, request);

                        if (result.Status == Const.SUCCESS_CREATE_CODE)
                        {
                            successResults.Add(result);
                        }
                        else
                        {
                            errors.Add(result.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Cannot add item to cart");
                        errors.Add($"Không thể thêm sản phẩm: {ex.Message}");
                    }
                }

                if (!successResults.Any() && errors.Any())
                {
                    return BadRequest(new
                    {
                        message = "Không có sản phẩm hợp lệ nào được thêm vào giỏ hàng.",
                        errors = errors
                    });
                }

                var response = new
                {
                    message = "Thêm sản phẩm thành công",
                    successCount = successResults.Count,
                    errorCount = errors.Count,
                    data = successResults,
                    errors = errors.Any() ? errors : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding multiple items to cart");
                return StatusCode(500, new { message = "Lỗi server khi thêm nhiều sản phẩm" });
            }
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        [HttpPut("update-items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var userId = User.GetUserId();

                if (request == null ||
                    (request.AccessoryQuantity is null && request.VariantQuantity is null && request.Quantity is null) ||
                    (request.AccessoryQuantity <= 0 && request.VariantQuantity <= 0 && request.Quantity <= 0))
                {
                    return BadRequest(new { message = "Thông tin cập nhật không hợp lệ." });
                }

                var result = await _cartService.UpdateItemAsync(userId, itemId, request);

                if (result.Status == Const.SUCCESS_UPDATE_CODE || result.Status == Const.SUCCESS_DELETE_CODE)
                    return Ok(result);

                if (result.Status == Const.WARNING_NO_DATA_CODE || result.Status == Const.FAIL_READ_CODE)
                    return NotFound(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {ItemId}", itemId);
                return StatusCode(500, new { message = "Lỗi server khi cập nhật sản phẩm" });
            }
        }

        /// <summary>
        /// Xóa một sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpDelete("delete-items/{itemId}")]
        [Authorize]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _cartService.RemoveFromCartAsync(userId, itemId);

                if (result.Status == Const.SUCCESS_DELETE_CODE)
                    return Ok(result);

                if (result.Status == Const.WARNING_NO_DATA_CODE)
                    return NotFound(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item {ItemId} from cart", itemId);
                return StatusCode(500, new { message = "Lỗi server khi xóa sản phẩm" });
            }
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng của người dùng
        /// </summary>
        [HttpDelete("delete-all-items")]
        [Authorize]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _cartService.ClearCartAsync(userId);

                if (result.Status == Const.SUCCESS_DELETE_CODE || result.Status == Const.WARNING_NO_DATA_CODE)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", User.GetUserId());
                return StatusCode(500, new { message = "Lỗi server khi xóa giỏ hàng" });
            }
        }

        /// <summary>
        /// Lấy tóm tắt giỏ hàng (số lượng, tổng tiền)
        /// </summary>
        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetCartSummary()
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _cartService.GetCartSummaryAsync(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary");
                return StatusCode(500, new { message = "Lỗi server khi lấy tóm tắt giỏ hàng" });
            }
        }

        /// <summary>
        /// Validate giỏ hàng (kiểm tra stock, giá cả)
        /// </summary>
        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateCart()
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _cartService.ValidateCartAsync(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart");
                return StatusCode(500, new { message = "Lỗi server khi validate giỏ hàng" });
            }
        }

        /// <summary>
        /// Tiến hành thanh toán cho giỏ hàng (Checkout)
        /// </summary>
        [HttpPost("checkout-cart")]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _cartService.CheckoutAsync(userId);

                if (result.Status == Const.SUCCESS_CREATE_CODE)
                    return Ok(result);

                if (result.Status == Const.FAIL_READ_CODE)
                    return BadRequest(result);

                if (result.Status == Const.BAD_REQUEST_CODE)
                    return BadRequest(result);

                return StatusCode(500, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for user {UserId}", User.GetUserId());
                return StatusCode(500, new { message = "Lỗi server khi checkout" });
            }
        }
    }

    [HttpPost("checkout/selected")]
    public async Task<IActionResult> CheckoutSelected([FromBody] CheckoutSelectedRequest req)
    {
        var userId =  User.GetUserId(); // Lấy userId từ JWT token
        
        var rs = await _cartService.CheckoutSelectedAsync(userId, req);
        return StatusCode(rs.Status, rs);
    }
    /// <summary>
    /// Lấy giỏ hàng đã HYDRATE (đủ ảnh/option như giao diện).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCartV2()
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _cartService.GetCartAsyncV2(userId); // bản bạn vừa sửa
            return StatusCode(result.Status, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BusinessResult(Const.ERROR_EXCEPTION, ex.Message));
        }
    }
    /// <summary>
    /// Cập nhật 1 dòng trong giỏ hàng theo kiểu ATOMIC (đổi variant/đổi terrarium/đổi số lượng) – 1 call duy nhất.
    /// </summary>
    [HttpPatch("items/{cartItemId:int}")]
    public async Task<IActionResult> PatchLine([FromRoute] int cartItemId, [FromBody] PatchCartLineRequest body)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _cartService.PatchLineAsync(userId, cartItemId, body); // bạn đã có hàm này trong service
            return StatusCode(result.Status, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new BusinessResult(Const.ERROR_EXCEPTION, ex.Message));
        }
    }

    
    

}
