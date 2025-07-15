using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.OrderItemDetail;

namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemDetailsController : ControllerBase
    {
        private readonly IOrderItemDetailService _orderItemDetailService;

        public OrderItemDetailsController(IOrderItemDetailService orderItemDetailService)
        {
            _orderItemDetailService = orderItemDetailService;
        }

        // Tạo OrderItemDetail mới (Chỉ Admin hoặc Manager mới có quyền)
        [HttpPost]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> CreateOrderItemDetail([FromBody] OrderItemDetailCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderItemDetailService.CreateOrderItemDetailAsync(
                request.OrderItemId,
                request.DetailKey,
                request.DetailValue,
                request.Quantity,
                request.UnitPrice
            );

            return Ok(result);
        }

        // Lấy tất cả chi tiết của một OrderItem (Chỉ Admin hoặc Staff mới có quyền)
        [HttpGet("order-item/{orderItemId}")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<IActionResult> GetOrderItemDetails(int orderItemId)
        {
            var result = await _orderItemDetailService.GetOrderItemDetailsByOrderItemIdAsync(orderItemId);
            if (result.Status != Const.SUCCESS_READ_CODE)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message, data = result.Data });
        }

        // Cập nhật OrderItemDetail (Chỉ Admin và Manager có quyền)
        [HttpPut("{id}")]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> UpdateOrderItemDetail(int id, [FromBody] OrderItemDetailUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _orderItemDetailService.UpdateOrderItemDetailAsync(
                id,
                request.DetailKey,
                request.DetailValue,
                request.Quantity,
                request.UnitPrice
            );

            return Ok(result);
        }

        // Xóa OrderItemDetail (Chỉ Admin có quyền)
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteOrderItemDetail(int id)
        {
            var result = await _orderItemDetailService.DeleteOrderItemDetailAsync(id);
            if (result.Status != Const.SUCCESS_DELETE_CODE)
            {
                return BadRequest(new { message = result.Message });
            }

            return NoContent();
        }
    }

}
