using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.MemberShip;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Service;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipController : ControllerBase
{
    private readonly IMembershipPackageService _membershipPackageService;
    private readonly IMembershipService _membershipService;
    private readonly IMomoServices _momoServices;
    private readonly IWalletServices _walletService;

    public MembershipController(IMembershipService membershipService,
        IMembershipPackageService membershipPackageService,
        IMomoServices momoServices,
        IWalletServices walletService)
    {
        _membershipService = membershipService;
        _membershipPackageService = membershipPackageService;
        _momoServices = momoServices;
        _walletService = walletService;
    }

  

    [HttpPost("momo/create-direct")]
    [Authorize]
    public async Task<IActionResult> CreateMembershipMomoDirect(
    [FromServices] IUserContextService userContext,
    [FromBody] DirectPaymentRequest request)
    {
        // Lấy user hiện tại thay vì tin payload
        var payload = new DirectPaymentRequest
        {
            UserId = userContext.GetCurrentUser(),
            PackageId = request.PackageId,
            StartDate = request.StartDate
        };

        var rsp = await _momoServices.CreateMomoMembershipDirectPaymentUrl(payload);
        return Ok(rsp);
    }

    [HttpPost("purchase")]
    [Authorize]
    public async Task<IBusinessResult> PurchaseMembership([FromBody] CreateMembershipRequest request)
    {
        try
        {
            // ✅ Validate payment method
            if (!new[] { "Momo", "Wallet" }.Contains(request.PaymentMethod))
            {
                return new BusinessResult(Const.BAD_REQUEST_CODE, "Phương thức thanh toán không hợp lệ. Chỉ hỗ trợ 'Momo' hoặc 'Wallet'.");
            }

            // Kiểm tra xem package có tồn tại không
            var package = await _membershipPackageService.GetByIdAsync(request.PackageId);
            if (package == null)
                return new BusinessResult(Const.NOT_FOUND_CODE, "Gói membership không tồn tại.");

            var activeMembership = _membershipService.IsMembershipExpired(new Membership() { UserId = request.UserId });
            if (!activeMembership)
            {
                return new BusinessResult(Const.BAD_REQUEST_CODE, "Không thể đăng ký gói mới vì gói hiện tại chưa hết hạn.");
            }

            // ✅ Xử lý theo payment method
            if (request.PaymentMethod == "Wallet")
            {
                // ✅ THANH TOÁN BẰNG VÍ
                return await ProcessWalletPayment(request, package);
            }
            else
            {
                // ✅ THANH TOÁN BẰNG MOMO (Logic cũ)
                return await ProcessMomoPayment(request, package);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return new BusinessResult(Const.UNAUTHORIZED_CODE, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return new BusinessResult(Const.BAD_REQUEST_CODE, ex.Message);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    // ✅ XỬ LÝ THANH TOÁN BẰNG VÍ
    private async Task<IBusinessResult> ProcessWalletPayment(CreateMembershipRequest request, dynamic package)
    {
        try
        {
            // Tạo membership và order
            var membershipResult = await _membershipService.CreateMembershipForUserAsync(
                request.UserId, request.PackageId, request.StartDate);

            // Xử lý thanh toán ví
            var walletPaymentResult = await _walletService.ProcessMembershipPaymentAsync(
                request.UserId, membershipResult.OrderId, package.Price);

            if (!walletPaymentResult.Success)
            {
                return new BusinessResult(Const.BAD_REQUEST_CODE, walletPaymentResult.Message);
            }
            await _membershipService.ActivateMembershipAsync(membershipResult.MembershipId);
            var responseData = new MembershipCreationResult
            {
                MembershipId = membershipResult.MembershipId,
                OrderId = membershipResult.OrderId,
                PaymentMethod = "Wallet",
                WalletPaymentInfo = walletPaymentResult.WalletInfo
            };

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Thanh toán membership bằng ví thành công", responseData);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, $"Lỗi thanh toán ví: {ex.Message}");
        }
    }

    // ✅ XỬ LÝ THANH TOÁN BẰNG MOMO (Logic cũ)
    private async Task<IBusinessResult> ProcessMomoPayment(CreateMembershipRequest request, dynamic package)
    {
        // Tạo membership cho người dùng
        var membershipId = await _membershipService.CreateMembershipForUserAsync(
            request.UserId, request.PackageId, request.StartDate);

        var momo = new MomoRequest
        {
            OrderId = membershipId.OrderId,
            PayAll = true
        };
        var rsp = await _momoServices.CreateMomoPaymentUrl(momo);

        var responseData = new MembershipCreationResult
        {
            MembershipId = membershipId.MembershipId,
            OrderId = membershipId.OrderId,
            PaymentMethod = "Momo",
            MomoQrResponse = rsp
        };

        return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Tạo membership cho người dùng thành công", responseData);
    }


    [HttpGet("all")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IBusinessResult> GetAllMemberships()
    {
        var memberships = await _membershipService.GetAllMembershipsAsync();
        if (memberships == null || memberships.Count == 0)
            return new BusinessResult(Const.ERROR_EXCEPTION, "Không tìm thấy memberships");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Thành công", memberships);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IBusinessResult> GetMembership(int id)
    {
        var membership = await _membershipService.GetMembershipByIdAsync(id);
        if (membership == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Membership không tồn tại");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Thành công", membership);
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IBusinessResult> GetMembershipsByUserId(int userId)
    {
        var memberships = await _membershipService.GetMembershipsByUserIdAsync(userId);
        if (memberships == null || memberships.Count == 0)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Không tìm thấy memberships cho người dùng này");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Thành công", memberships);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IBusinessResult> UpdateMembership(int id, [FromBody] CreateMembershipRequest request)
    {
        try
        {
            // Sửa lại việc sử dụng MembershipType
            var success = await _membershipService.UpdateMembershipAsync(id, request.PackageId, request.StartDate);
            if (!success)
                return new BusinessResult(Const.NOT_FOUND_CODE, "Membership không tồn tại hoặc không thể cập nhật");

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật thành công", id);
        }
        catch (ArgumentException ex)
        {
            return new BusinessResult(Const.BAD_REQUEST_CODE, ex.Message);
        }
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IBusinessResult> DeleteMembership(int id)
    {
        var success = await _membershipService.DeleteMembershipAsync(id);
        if (!success)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Membership không tồn tại hoặc không thể xóa");

        return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Xóa thành công", id);
    }

    [HttpPost("update-expired")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IBusinessResult> UpdateAllExpiredMemberships()
    {
        var updatedCount = await _membershipService.UpdateAllExpiredMembershipsAsync();
        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Đã cập nhật trạng thái hết hạn", updatedCount);
    }

    [HttpPost("user/{userId}/update-expired")]
    [Authorize]
    public async Task<IBusinessResult> UpdateExpiredMembershipsByUserId(int userId)
    {
        var updatedCount = await _membershipService.UpdateExpiredMembershipsByUserIdAsync(userId);
        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Đã cập nhật trạng thái hết hạn", updatedCount);
    }

    [HttpGet("{id}/is-expired")]
    [Authorize]
    public async Task<IBusinessResult> CheckIfMembershipIsExpired(int id)
    {
        var membership = await _membershipService.GetMembershipByIdAsync(id);
        if (membership == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Membership không tồn tại");

        var isExpired = _membershipService.IsMembershipExpired(membership);
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Thành công", new { isExpired });
    }
}