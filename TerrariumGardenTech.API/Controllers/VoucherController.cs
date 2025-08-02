using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Voucher;
using TerrariumGardenTech.Common.ResponseModel.Voucher;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

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
    [HttpGet("get-by-code/{code}")]
    public async Task<IActionResult> GetVoucherByCode(string code)
    {
        var voucher = await _voucherService.GetVoucherByCodeAsync(code);
        if (voucher == null)
            return NotFound();

        // Chuyển đổi các kiểu dữ liệu một cách rõ ràng
        var response = new VoucherResponse
        {
            VoucherId = voucher.VoucherId,
            Code = voucher.Code,
            Description = voucher.Description,
            DiscountAmount = voucher.DiscountAmount ?? 0m, // Xử lý nullable
            ValidFrom = voucher.ValidFrom ?? DateTime.MinValue, // Xử lý nullable
            ValidTo = voucher.ValidTo ?? DateTime.MinValue, // Xử lý nullable
            Status = Enum.TryParse<VoucherStatus>(voucher.Status, out var status)
                ? status
                : VoucherStatus.Inactive // Chuyển đổi string sang enum
        };

        return Ok(response);
    }

    // Thêm Voucher
    [HttpPost]
    public async Task<IActionResult> AddVoucher([FromBody] CreateVoucherRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var voucher = new Voucher
        {
            Code = request.Code,
            Description = request.Description,
            DiscountAmount = request.DiscountAmount,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            Status = request.Status.ToString()
        };

        await _voucherService.AddVoucherAsync(voucher);
        return CreatedAtAction(nameof(GetVoucherByCode), new { code = voucher.Code }, voucher);
    }

    // Cập nhật Voucher
    [HttpPut("update-voucher/{id}")]
    public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != request.VoucherId)
            return BadRequest("Voucher ID mismatch.");

        var voucher = new Voucher
        {
            VoucherId = request.VoucherId,
            Code = request.Code,
            Description = request.Description,
            DiscountAmount = request.DiscountAmount,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            Status = request.Status.ToString()
        };

        await _voucherService.UpdateVoucherAsync(voucher);
        return NoContent();
    }

    // Xóa Voucher
    [HttpDelete("delete-voucher/{id}")]
    public async Task<IActionResult> DeleteVoucher(int id)
    {
        await _voucherService.DeleteVoucherAsync(id);
        return NoContent();
    }
}