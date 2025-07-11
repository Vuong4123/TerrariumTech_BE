using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Order;

namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _svc;
        private readonly IAuthorizationService _auth;

        public OrderController(IOrderService svc, IAuthorizationService auth)
        {
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        /// <summary>
        /// Lấy danh sách tất cả đơn hàng (quyền Order.ReadAll)
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
        /// Lấy chi tiết đơn hàng theo ID
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
        /// Tạo mới đơn hàng
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
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        [HttpPut("{id:int}/status")]
        [Authorize(Policy = "Order.UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new ArgumentException("Status không được để trống.", nameof(status));

                var updated = await _svc.UpdateStatusAsync(id, status.Trim());
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
        /// Xóa đơn hàng
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
}
