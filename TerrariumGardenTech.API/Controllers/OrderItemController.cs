using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.OrderItem;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderItemController : ControllerBase
{
    private readonly IAuthorizationService _auth;
    private readonly IOrderItemService _svc;

    public OrderItemController(IOrderItemService svc, IAuthorizationService auth)
    {
        _svc = svc;
        _auth = auth;
    }

    /// <summary>
    ///     Lấy danh sách tất cả OrderItem
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
    ///     Lấy chi tiết một OrderItem theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var item = await _svc.GetByIdAsync(id);
            if (item is null)
                return NotFound(new { message = $"OrderItem ({id}) không tồn tại." });

            var authResult = await _auth.AuthorizeAsync(User, item.OrderId, "Order.AccessSpecific");
            if (!authResult.Succeeded)
                return Forbid();

            return Ok(item);
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
    ///     Lấy tất cả OrderItem của một đơn hàng
    /// </summary>
    [HttpGet("by-order/{orderId:int}")]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        try
        {
            var authResult = await _auth.AuthorizeAsync(User, orderId, "Order.AccessSpecific");
            if (!authResult.Succeeded)
                return Forbid();

            var list = await _svc.GetByOrderAsync(orderId);
            if (!list.Any())
                return NotFound(new { message = $"Đơn hàng ({orderId}) không tồn tại hoặc chưa có item." });

            return Ok(list);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new { message = knf.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    ///     Tạo mới một OrderItem
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderItemRequest req)
    {
        try
        {
            var authResult = await _auth.AuthorizeAsync(User, req.OrderId, "Order.AccessSpecific");
            if (!authResult.Succeeded)
                return Forbid();

            var id = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(Get), new { id }, new { orderItemId = id });
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new { message = knf.Message });
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
    ///     Cập nhật thông tin một OrderItem
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateOrderItemRequest req)
    {
        try
        {
            var existing = await _svc.GetByIdAsync(id);
            if (existing is null)
                return NotFound(new { message = $"OrderItem ({id}) không tồn tại." });

            var authResult = await _auth.AuthorizeAsync(User, existing.OrderId, "Order.AccessSpecific");
            if (!authResult.Succeeded)
                return Forbid();

            var updated = await _svc.UpdateAsync(id, req);
            return updated ? NoContent() : NotFound(new { message = "Cập nhật thất bại." });
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new { message = knf.Message });
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
    ///     Xóa một OrderItem
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var existing = await _svc.GetByIdAsync(id);
            if (existing is null)
                return NotFound(new { message = $"OrderItem ({id}) không tồn tại." });

            var authResult = await _auth.AuthorizeAsync(User, existing.OrderId, "Order.AccessSpecific");
            if (!authResult.Succeeded)
                return Forbid();

            var deleted = await _svc.DeleteAsync(id);
            return deleted ? NoContent() : NotFound(new { message = "Xóa thất bại." });
        }
        catch (ArgumentException ae)
        {
            return BadRequest(new { message = ae.Message });
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new { message = knf.Message });
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
}