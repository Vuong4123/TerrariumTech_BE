using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IAuthorizationService _auth;
    private readonly IOrderService _svc;

    public OrderController(IOrderService svc, IAuthorizationService auth)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        _auth = auth ?? throw new ArgumentNullException(nameof(auth));
    }

    /// <summary>
    ///     Lấy danh sách tất cả đơn hàng (quyền Order.ReadAll)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Order.ReadAll")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var list = await _svc.GetAllAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Lấy chi tiết đơn hàng theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var authResult = await _auth.AuthorizeAsync(User, id, "Order.AccessSpecific");
            if (!authResult.Succeeded)
                return Forbid();

            var order = await _svc.GetByIdAsync(id);
            if (order is null)
                return NotFound(new { message = $"Đơn hàng ({id}) không tồn tại." });

            return Ok(order);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Lấy chi tiết đơn hàng theo ID
    /// </summary>
    [HttpGet("getbyuserid{id:int}")]
    
    public async Task<IActionResult> GetByUserdId(int userId)
    {
        try
        {
            

            var order = await _svc.GetByUserAsync(userId);
            if (order is null)
                return NotFound(new { message = $"Đơn hàng ({userId}) không tồn tại." });

            return Ok(order);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Tạo mới đơn hàng
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] OrderCreateRequest req)
    {
        try
        {
            req.UserId = User.GetUserId();
            var id = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(Get), new { id }, new { orderId = id });
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (InvalidOperationException ioe)
        {
            return Conflict(new { message = ioe.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }


    /// <summary>
    ///     Cập nhật trạng thái đơn hàng
    /// </summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "Order.UpdateStatus")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus status)
    {
        try
        {
            // gọi service với enum, không cần trim
            var updated = await _svc.UpdateStatusAsync(id, status);
            if (!updated)
                return NotFound(new { message = $"Không tìm thấy đơn hàng ({id}) để cập nhật." });

            return NoContent();
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Xung đột dữ liệu, vui lòng thử lại." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Xử lý thanh toán đơn hàng
    /// </summary>
    [HttpPost("{id:int}/checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout(int id, [FromBody] CheckoutRequest checkoutRequest)
    {
        try
        {
            var result = await _svc.CheckoutAsync(id, checkoutRequest.PaymentMethod);
            if (result.Status == Const.SUCCESS_UPDATE_CODE)
                return Ok(new { message = result.Message, statusCode = result.Status });

            return BadRequest(new { message = result.Message, statusCode = result.Status });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Lấy danh sách đơn hàng theo UserId
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [Authorize(Roles = "User,Staff,Manager,Admin,Shipper")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        try
        {
            var currentUserId = User.GetUserId();

            // Nếu không phải chủ đơn, phải có quyền Order.ReadAll mới xem được đơn của người khác
            if (currentUserId != userId)
            {
                var authResult = await _auth.AuthorizeAsync(User, null, "Order.ReadAll");
                if (!authResult.Succeeded)
                    return Forbid();
            }

            var orders = await _svc.GetByUserAsync(userId);
            return Ok(orders);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }



    /// <summary>
    ///     Xóa đơn hàng
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Order.Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _svc.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Không tìm thấy đơn hàng ({id}) để xóa." });

            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Không thể xóa đơn hàng, vui lòng thử lại." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}