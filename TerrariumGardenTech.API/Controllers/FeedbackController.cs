using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.Feedback;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;

        public FeedbackController(IFeedbackService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FeedbackCreateRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetByOrderItem),
                                   new { orderItemId = result.OrderItemId }, result);
        }

        [HttpGet("{orderItemId:int}")]
        public async Task<IActionResult> GetByOrderItem(int orderItemId)
        {
            var list = await _service.GetByOrderItemAsync(orderItemId);
            return Ok(list);
        }
    }
}
