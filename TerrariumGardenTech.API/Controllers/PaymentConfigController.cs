using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.PaymentConfig;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentConfigController : ControllerBase
    {
        private readonly IPaymentConfigService _paymentConfigService;

        public PaymentConfigController(IPaymentConfigService paymentConfigService)
        {
            _paymentConfigService = paymentConfigService;
        }

        // GET: api/paymentconfig
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var configs = await _paymentConfigService.GetAllAsync();
            return Ok(configs);
        }

        // GET: api/paymentconfig/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var config = await _paymentConfigService.GetByIdAsync(id);
            if (config == null)
                return NotFound(new { message = "Không tìm thấy cấu hình" });
            return Ok(config);
        }

        // POST: api/paymentconfig
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentConfigRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _paymentConfigService.CreateAsync(request);
            return Ok(new { id });
        }

        // PUT: api/paymentconfig/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentConfigRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _paymentConfigService.UpdateAsync(id, request);
            if (!result)
                return NotFound(new { message = "Không tìm thấy cấu hình" });
            return Ok(new { message = "Cập nhật thành công" });
        }

        // DELETE: api/paymentconfig/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _paymentConfigService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Không tìm thấy cấu hình" });
            return Ok(new { message = "Xóa thành công" });
        }
    }
}