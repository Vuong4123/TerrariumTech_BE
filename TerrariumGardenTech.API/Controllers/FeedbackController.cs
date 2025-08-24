using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TerrariumGardenTech.Common.RequestModel.Feedback;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _svc;
        public FeedbackController(IFeedbackService svc) => _svc = svc;

        int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FeedbackCreateRequest req)
        {
            var res = await _svc.CreateAsync(req, UserId);
            return CreatedAtAction(nameof(GetByOrderItem),
                                   new { orderItemId = res.OrderItemId }, res);
        }

        // Lấy feedback theo terrarium với phân trang
        [HttpGet("terrarium/{terrariumId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByTerrarium(int terrariumId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _svc.GetByTerrariumAsync(terrariumId, page, pageSize);
            Response.Headers.Add("X-Total-Count", total.ToString());
            return Ok(items);
        }
        // Lấy feedback theo terrarium với phân trang
        [HttpGet("accessory/{accessoryId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByAccessory(int accessoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _svc.GetByAccessoryAsync(accessoryId, page, pageSize);
            Response.Headers.Add("X-Total-Count", total.ToString());
            return Ok(items);
        }
        // Lấy feedback theo terrarium với phân trang
        [HttpGet("user/{userId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _svc.GetAllByUserAsync(userId, page, pageSize);
            Response.Headers.Add("X-Total-Count", total.ToString());
            return Ok(items);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _svc.GetAllAsync(page, pageSize);
            Response.Headers.Add("X-Total-Count", total.ToString());
            return Ok(items);
        }

        [HttpGet("order/{orderItemId:int}")]
        public async Task<IActionResult> GetByOrderItem(int orderItemId)
            => Ok(await _svc.GetByOrderItemAsync(orderItemId));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] FeedbackUpdateRequest req)
            => Ok(await _svc.UpdateAsync(id, req, UserId));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id, UserId);
            return ok ? NoContent() : NotFound();
        }
    }
}
