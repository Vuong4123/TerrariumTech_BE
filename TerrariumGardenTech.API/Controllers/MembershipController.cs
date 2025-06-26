using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.MemberShip;
using System.Threading.Tasks;

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

        // API Create Membership
        [HttpPost("create")]
        public async Task<IActionResult> CreateMembership([FromBody] CreateMembershipRequest request)
        {
            var membershipId = await _membershipService.CreateMembershipAsync(request.UserId, request.MembershipType, request.StartDate, request.EndDate, request.Status);
            return CreatedAtAction(nameof(GetMembership), new { id = membershipId }, request);
        }

        // API Get Membership by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMembership(int id)
        {
            var membership = await _membershipService.GetMembershipByIdAsync(id);
            if (membership == null)
                return NotFound(new { message = "Membership không tồn tại" });
            return Ok(membership);
        }

        // API Get all memberships of a user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMembershipsByUserId(int userId)
        {
            var memberships = await _membershipService.GetMembershipsByUserIdAsync(userId);
            if (memberships == null || memberships.Count == 0)
                return NotFound(new { message = "Không tìm thấy memberships cho người dùng này" });
            return Ok(memberships);
        }

        // API Update Membership
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMembership(int id, [FromBody] CreateMembershipRequest request)
        {
            var success = await _membershipService.UpdateMembershipAsync(id, request.MembershipType, request.StartDate, request.EndDate, request.Status);
            if (!success)
                return NotFound(new { message = "Membership không tồn tại hoặc không thể cập nhật" });
            return NoContent();  // Thành công, nhưng không trả về dữ liệu
        }

        // API Delete Membership
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembership(int id)
        {
            var success = await _membershipService.DeleteMembershipAsync(id);
            if (!success)
                return NotFound(new { message = "Membership không tồn tại hoặc không thể xóa" });
            return NoContent();  // Xóa thành công
        }

        // API Update all expired memberships
        [HttpPost("update-expired")]
        public async Task<IActionResult> UpdateAllExpiredMemberships()
        {
            var updatedCount = await _membershipService.UpdateAllExpiredMembershipsAsync();
            return Ok(new { updatedCount });
        }

        // API Update expired memberships for a specific user
        [HttpPost("user/{userId}/update-expired")]
        public async Task<IActionResult> UpdateExpiredMembershipsByUserId(int userId)
        {
            var updatedCount = await _membershipService.UpdateExpiredMembershipsByUserIdAsync(userId);
            return Ok(new { updatedCount });
        }

        // API Check if a membership is expired
        [HttpGet("{id}/is-expired")]
        public IActionResult CheckIfMembershipIsExpired(int id)
        {
            var membership = _membershipService.GetMembershipByIdAsync(id).Result;
            if (membership == null)
                return NotFound(new { message = "Membership không tồn tại" });

            bool isExpired = _membershipService.IsMembershipExpired(membership);
            return Ok(new { isExpired });
        }
    }
}
