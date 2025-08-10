using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TerrariumGardenTech.Common.RequestModel.UploadImage;
using TerrariumGardenTech.Common.RequestModel.Voucher;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) return null;
            if (int.TryParse(userIdClaim, out var userId)) return userId;
            return null;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue) return Unauthorized(new { message = "Token không hợp lệ" });

            var rs = await _profileService.GetMyProfileAsync(userId.Value);
            return StatusCode(rs.Status, rs);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> EditMyProfile([FromBody] EditUserProfileRequest req)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue) return Unauthorized(new { message = "Token không hợp lệ" });

            var rs = await _profileService.EditMyProfileAsync(userId.Value, req);
            return StatusCode(rs.Status, rs);
        }

        [Authorize]
        [HttpPost("me/avatar")]
        [RequestSizeLimit(10_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar([FromForm] FileUploadModel model)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue) return Unauthorized(new { message = "Token không hợp lệ" });

            var rs = await _profileService.UploadAvatarAsync(userId.Value, model.File);
            return StatusCode(rs.Status, rs);
        }

        [Authorize]
        [HttpPost("me/background")]
        [RequestSizeLimit(10_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadBackground([FromForm] FileUploadModel model)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue) return Unauthorized(new { message = "Token không hợp lệ" });

            var rs = await _profileService.UploadBackgroundAsync(userId.Value, model.File);
            return StatusCode(rs.Status, rs);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> AdminGetAllProfiles()
        {
            var result = await _profileService.AdminGetAllProfilesAsync();
            return StatusCode(result.Status, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> AdminGetProfileByUserId(int userId)
        {
            var result = await _profileService.AdminGetProfileByUserIdAsync(userId);
            return StatusCode(result.Status, result);
        }
    }
}
