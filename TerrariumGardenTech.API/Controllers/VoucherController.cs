using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Voucher;
using TerrariumGardenTech.Common.ResponseModel.Voucher;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // bắt buộc đăng nhập cho toàn bộ controller
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;
    public VoucherController(IVoucherService voucherService) => _voucherService = voucherService;

    private string? CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst("uid")?.Value;

    private bool IsAdminOrManager =>
        User.IsInRole("Admin") || User.IsInRole("Manager");

    // ========= GET ALL =========
    // Admin/Manager xem toàn bộ; user thường cũng có thể xem (tuỳ business).
    // Nếu bạn muốn chỉ admin/manager được xem toàn bộ, thêm [Authorize(Roles="Admin,Manager")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _voucherService.GetAllAsync(ct);
        var resp = list.Select(v => new VoucherResponse
        {
            VoucherId = v.VoucherId,
            Code = v.Code,
            Description = v.Description,
            DiscountAmount = v.DiscountAmount,
            DiscountPercent = v.DiscountPercent,
            ValidFrom = v.ValidFrom,
            ValidTo = v.ValidTo,
            Status = Enum.TryParse<VoucherStatus>(v.Status, true, out var st) ? st : VoucherStatus.Inactive,
            IsPersonal = v.IsPersonal,
            TargetUserId = v.TargetUserId,
            TotalUsage = v.TotalUsage,
            RemainingUsage = v.RemainingUsage,
            PerUserUsageLimit = v.PerUserUsageLimit,
            MinOrderAmount = v.MinOrderAmount
        });
        return Ok(resp);
    }

    // ========= GET BY CODE =========
    [HttpGet("get-by-code/{code}")]
    public async Task<IActionResult> GetVoucherByCode(string code, CancellationToken ct)
    {
        var v = await _voucherService.GetByCodeAsync(code, ct);
        if (v == null) return NotFound();

        // Nếu là voucher cá nhân, chỉ chủ sở hữu, Admin/Manager mới được xem chi tiết
        if (v.IsPersonal && !IsAdminOrManager)
        {
            if (string.IsNullOrWhiteSpace(CurrentUserId) || !string.Equals(v.TargetUserId, CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return Forbid();
        }

        var resp = new VoucherResponse
        {
            VoucherId = v.VoucherId,
            Code = v.Code,
            Description = v.Description,
            DiscountAmount = v.DiscountAmount,
            DiscountPercent = v.DiscountPercent,
            ValidFrom = v.ValidFrom,
            ValidTo = v.ValidTo,
            Status = Enum.TryParse<VoucherStatus>(v.Status, true, out var st) ? st : VoucherStatus.Inactive,
            IsPersonal = v.IsPersonal,
            TargetUserId = v.TargetUserId,
            TotalUsage = v.TotalUsage,
            RemainingUsage = v.RemainingUsage,
            PerUserUsageLimit = v.PerUserUsageLimit,
            MinOrderAmount = v.MinOrderAmount
        };
        return Ok(resp);
    }

    // ========= CREATE =========
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddVoucher([FromBody] CreateVoucherRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _voucherService.IsCodeExistAsync(req.Code))
        {
            return BadRequest(new { message = $"Mã voucher '{req.Code}' đã tồn tại" });
        }
        var v = new Voucher
        {
            Code = req.Code,
            Description = req.Description,
            DiscountAmount = req.DiscountAmount,
            DiscountPercent = req.DiscountPercent,
            ValidFrom = req.ValidFrom,
            ValidTo = req.ValidTo,
            Status = req.Status.ToString(),

            IsPersonal = req.IsPersonal,
            TargetUserId = req.TargetUserId,
            TotalUsage = req.TotalUsage,
            RemainingUsage = req.TotalUsage,
            PerUserUsageLimit = req.PerUserUsageLimit,
            MinOrderAmount = req.MinOrderAmount
        };

        var created = await _voucherService.CreateAsync(v, ct);
        return CreatedAtAction(nameof(GetVoucherByCode), new { code = created.Code }, created);
    }

    // ========= UPDATE =========
    [HttpPut("update-voucher/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherRequest req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await _voucherService.IsCodeExistAsync(req.Code, id))
        {
            return BadRequest(new { message = $"Mã voucher '{req.Code}' đã được sử dụng bởi voucher khác" });
        }
        if (id != req.VoucherId) return BadRequest("Voucher ID mismatch.");

        var v = new Voucher
        {
            VoucherId = req.VoucherId,
            Code = req.Code,
            Description = req.Description,
            DiscountAmount = req.DiscountAmount,
            DiscountPercent = req.DiscountPercent,
            ValidFrom = req.ValidFrom,
            ValidTo = req.ValidTo,
            Status = req.Status.ToString(),

            IsPersonal = req.IsPersonal,
            TargetUserId = req.TargetUserId,
            TotalUsage = req.TotalUsage,
            RemainingUsage = req.RemainingUsage,
            PerUserUsageLimit = req.PerUserUsageLimit,
            MinOrderAmount = req.MinOrderAmount
        };

        await _voucherService.UpdateAsync(v, ct);
        return NoContent();
    }

    // Validate voucher với order amount
    [HttpGet("validate/{code}/order-amount/{orderAmount}")]
    public async Task<IActionResult> ValidateVoucherWithOrderAmount(string code, decimal orderAmount, CancellationToken ct)
    {
        var v = await _voucherService.GetByCodeAsync(code, ct);
        if (v == null) return BadRequest(new { valid = false, reason = "Voucher không tồn tại." });

        var now = DateTime.UtcNow.Date;
        var active = Enum.TryParse<VoucherStatus>(v.Status, true, out var st) && st == VoucherStatus.Active
                     && (v.ValidFrom == null || v.ValidFrom <= now)
                     && (v.ValidTo == null || v.ValidTo >= now)
                     && v.RemainingUsage > 0;

        if (!active) return BadRequest(new { valid = false, reason = "Voucher không còn hiệu lực." });

        // ✅ KIỂM TRA MIN ORDER AMOUNT
        if (v.MinOrderAmount.HasValue && orderAmount < v.MinOrderAmount.Value)
            return BadRequest(new { valid = false, reason = $"Đơn hàng tối thiểu {v.MinOrderAmount.Value:N0}đ để sử dụng voucher này." });

        if (v.IsPersonal && !IsAdminOrManager)
        {
            if (string.IsNullOrWhiteSpace(CurrentUserId) || !string.Equals(v.TargetUserId, CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { valid = false, reason = "Voucher cá nhân, không thuộc về bạn." });
        }

        return Ok(new { valid = true });
    }
    // ========= DELETE =========
    [HttpDelete("delete-voucher/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteVoucher(int id, CancellationToken ct)
    {
        await _voucherService.DeleteAsync(id, ct);
        return NoContent();
    }

    // ========= VALIDATE =========
    // Nếu voucher là personal thì bắt buộc user hiện tại phải là chủ sở hữu (hoặc admin/manager)
    [HttpGet("validate/{code}")]
    public async Task<IActionResult> ValidateVoucher(string code, CancellationToken ct)
    {
        var v = await _voucherService.GetByCodeAsync(code, ct);
        if (v == null) return BadRequest(new { valid = false, reason = "Voucher không tồn tại." });

        var now = DateTime.UtcNow.Date;
        var active = Enum.TryParse<VoucherStatus>(v.Status, true, out var st) && st == VoucherStatus.Active
                     && (v.ValidFrom == null || v.ValidFrom <= now)
                     && (v.ValidTo == null || v.ValidTo >= now)
                     && v.RemainingUsage > 0;

        if (!active) return BadRequest(new { valid = false, reason = "Voucher không còn hiệu lực." });

        if (v.IsPersonal && !IsAdminOrManager)
        {
            if (string.IsNullOrWhiteSpace(CurrentUserId) || !string.Equals(v.TargetUserId, CurrentUserId, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { valid = false, reason = "Voucher cá nhân, không thuộc về bạn." });
        }

        return Ok(new { valid = true });
    }

    // ========= CONSUME =========
    // Chỉ cho phép: chủ sở hữu (nếu personal) hoặc Admin/Manager
    public record ConsumeRequest(string? UserId);

    [HttpPost("{code}/consume")]
    public async Task<IActionResult> Consume(string code, [FromBody] ConsumeRequest body, CancellationToken ct)
    {
        var v = await _voucherService.GetByCodeAsync(code, ct);
        if (v == null) return BadRequest(new { success = false, message = "Voucher không tồn tại." });

        var callerUserId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(callerUserId))
            return Unauthorized(new { success = false, message = "Bạn chưa đăng nhập." });

        // Nếu là voucher cá nhân: chỉ đúng chủ sở hữu hoặc admin/manager mới được consume
        if (v.IsPersonal && !IsAdminOrManager && !string.Equals(v.TargetUserId, callerUserId, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        // Không cho phép “mạo danh” userId trong body; dùng id của người gọi
        var userIdToConsume = callerUserId;

        var (ok, message, remaining, userUsed) = await _voucherService.ConsumeAsync(code, userIdToConsume!, ct);
        return Ok(new { success = ok, message, remainingUsage = remaining, userUsedCount = userUsed });
    }
}
