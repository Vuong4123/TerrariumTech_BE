using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Image;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinary;

        public ImageController(ICloudinaryService cloudinary)
        {
            _cloudinary = cloudinary;
        }

        /// <summary>
        /// Upload 1 ảnh lên Cloudinary (multipart/form-data)
        /// </summary>
        /// <param name="file">Tệp ảnh</param>
        /// <param name="folder">Thư mục lưu trên Cloudinary (vd: "avatars", "products/terrarium")</param>
        /// <param name="publicId">PublicId tùy chọn (vd: "avatar_123")</param>
        [HttpPost("upload")]
        [Authorize] // nếu không cần auth thì bỏ
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)] // 5MB
        public async Task<IActionResult> UploadImage([FromForm] ImageUploadRequest request)
        {
            // Lấy userId từ claim (ví dụ claim name là "sub" hoặc "userId")
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Không xác định được userId trong token." });

            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { message = "File rỗng hoặc không được gửi lên." });

            var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(request.File.ContentType, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Chỉ cho phép JPEG/PNG/WEBP/GIF." });

            const long maxBytes = 5L * 1024 * 1024;
            if (request.File.Length > maxBytes)
                return BadRequest(new { message = "Dung lượng tối đa 5MB." });

            // Gán cứng folder và publicId
            var folder = "uploads_image_chat";
            var publicId = userId.ToString();

            var br = await _cloudinary.UploadImageAsync(request.File, folder, publicId);

            return br.Status switch
            {
                var s when s == Const.SUCCESS_CREATE_CODE
                    => Ok(new { message = br.Message, url = br.Data }),

                var s when s == Const.FAIL_CREATE_CODE
                    => BadRequest(new { message = br.Message }),

                var s when s == Const.ERROR_EXCEPTION
                    => StatusCode(StatusCodes.Status500InternalServerError,
                                  new { message = "Lỗi hệ thống", detail = br.Message }),

                _ => BadRequest(new { message = br.Message })
            };
        }
    }
}
