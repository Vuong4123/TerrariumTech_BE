using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        // Kiểm tra Voucher có hợp lệ không
        [HttpGet("validate/{code}")]
        public async Task<IActionResult> ValidateVoucher(string code)
        {
            var isValid = await _voucherService.IsVoucherValidAsync(code);
            if (isValid)
                return Ok("Voucher is valid.");
            return BadRequest("Voucher is not valid.");
        }

        // Lấy Voucher theo mã code
        [HttpGet("{code}")]
        public async Task<IActionResult> GetVoucherByCode(string code)
        {
            var voucher = await _voucherService.GetVoucherByCodeAsync(code);
            if (voucher == null)
                return NotFound();
            return Ok(voucher);
        }

        // Thêm Voucher
        [HttpPost]
        public async Task<IActionResult> AddVoucher([FromBody] Voucher voucher)
        {
            await _voucherService.AddVoucherAsync(voucher);
            return CreatedAtAction(nameof(GetVoucherByCode), new { code = voucher.Code }, voucher);
        }

        // Cập nhật Voucher
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVoucher(int id, [FromBody] Voucher voucher)
        {
            if (id != voucher.VoucherId)
                return BadRequest();

            await _voucherService.UpdateVoucherAsync(voucher);
            return NoContent();
        }

        // Xóa Voucher
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            await _voucherService.DeleteVoucherAsync(id);
            return NoContent();
        }
    }
}
