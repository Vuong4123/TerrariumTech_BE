using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.MemberShip;


namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        // Dành cho Admin tạo membership cho user (nếu mở rộng theo userId)
        [HttpPost("create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IBusinessResult> CreateMembership([FromBody] CreateMembershipRequest request)
        {
            try
            {
                var membershipId = await _membershipService.CreateMembershipAsync(request.MembershipType, request.StartDate);
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Tạo membership thành công", membershipId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return new BusinessResult(Const.UNAUTHORIZED_CODE, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return new BusinessResult(Const.BAD_REQUEST_CODE, ex.Message);
            }
        }

        // Người dùng tự mua membership
        [HttpPost("purchase")]
        [Authorize]
        public async Task<IBusinessResult> PurchaseMembership([FromBody] CreateMembershipRequest request)
        {
            try
            {
                var membershipId = await _membershipService.CreateMembershipAsync(request.MembershipType, request.StartDate);
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Mua membership thành công", membershipId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return new BusinessResult(Const.UNAUTHORIZED_CODE, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return new BusinessResult(Const.BAD_REQUEST_CODE, ex.Message);
            }
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
        [Authorize]
        public async Task<IBusinessResult> UpdateMembership(int id, [FromBody] CreateMembershipRequest request)
        {
            try
            {
                var success = await _membershipService.UpdateMembershipAsync(id, request.MembershipType, request.StartDate);
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
}
