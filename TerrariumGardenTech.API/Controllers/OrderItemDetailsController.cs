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

        // Tạo OrderItemDetail mới
        [HttpPost]
        public async Task<IActionResult> CreateOrderItemDetail([FromBody] OrderItemDetailCreateRequest request)
        {
            var result = await _orderItemDetailService.CreateOrderItemDetailAsync(
                request.OrderItemId,
                request.DetailKey,
                request.DetailValue
            );

            if (result.Status != Const.SUCCESS_CREATE_CODE)
            {
                return BadRequest(new { message = result.Message });
            }

            return CreatedAtAction(nameof(GetOrderItemDetails), new { id = result.Data.orderItemDetailId }, new { message = result.Message, data = result.Data });
        }

        // Lấy tất cả chi tiết của một OrderItem
        [HttpGet("order-item/{orderItemId}")]
        public async Task<IActionResult> GetOrderItemDetails(int orderItemId)
        {
            var result = await _orderItemDetailService.GetOrderItemDetailsByOrderItemIdAsync(orderItemId);
            if (result.Status != Const.SUCCESS_READ_CODE)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message, data = result.Data });
        }

        // Cập nhật OrderItemDetail
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderItemDetail(int id, [FromBody] OrderItemDetailUpdateRequest request)
        {
            var result = await _orderItemDetailService.UpdateOrderItemDetailAsync(id, request.DetailKey, request.DetailValue);
            if (result.Status != Const.SUCCESS_UPDATE_CODE)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message, data = result.Data });
        }

        // Xóa OrderItemDetail
        [HttpDelete("{id}")]
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
