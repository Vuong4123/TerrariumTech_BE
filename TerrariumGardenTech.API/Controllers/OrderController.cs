using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            _svc = svc;
            _auth = auth;
        }

        // GET all
        [Authorize(Policy = "Order.ReadAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _svc.GetAllAsync());

        // GET by id
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _auth.AuthorizeAsync(User, id, "Order.AccessSpecific");
            if (!result.Succeeded) return Forbid();

            var order = await _svc.GetByIdAsync(id);
            return order is null ? NotFound() : Ok(order);
        }

        // POST create
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateRequest req)
        {
            req.UserId = User.GetUserId();
            var id = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(Get), new { id }, new { orderId = id });
        }

        // PUT status
        [Authorize(Policy = "Order.UpdateStatus")]
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status) =>
            await _svc.UpdateStatusAsync(id, status) ? NoContent() : NotFound();

        // DELETE
        [Authorize(Policy = "Order.Delete")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id) =>
            await _svc.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
