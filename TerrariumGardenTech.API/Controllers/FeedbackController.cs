using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TerrariumGardenTech.Common.RequestModel.Feedback;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]   // Bắt buộc xác thực để có Claim
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;

        public FeedbackController(IFeedbackService service)
            => _service = service;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FeedbackCreateRequest request)
        {
            // 1. Lấy claim userId (NameIdentifier thường là claim sub/nameid)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid token" });

            // 2. Gọi service
            var result = await _service.CreateAsync(request, userId);

            // 3. Trả về Created 201
            return CreatedAtAction(nameof(GetByOrderItem),
                                   new { orderItemId = result.OrderItemId },
                                   result);
        }

        [HttpGet("{orderItemId:int}")]
        public async Task<IActionResult> GetByOrderItem(int orderItemId)
        {
            var list = await _service.GetByOrderItemAsync(orderItemId);
            return Ok(list);
        }
    }
}
