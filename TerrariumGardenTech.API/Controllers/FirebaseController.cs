using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Firebase;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirebaseController : ControllerBase
    {
        private readonly IFirebaseStorageService _firebaseStorageService;

        public FirebaseController(IFirebaseStorageService firebaseStorageService)
        {
            _firebaseStorageService = firebaseStorageService;
        }

        [HttpPost("save-fcmtoken")]
        public async Task<IActionResult> SaveToken([FromBody] CreateTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.FcmToken))
            {
                return BadRequest("UserId và Token không được để trống.");
            }

            try
            {
                await _firebaseStorageService.SaveTokenAsync(request.UserId, request.FcmToken);
                return Ok("FCMToken đã được lưu thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lưu fcmtoken: {ex.Message}");
            }
        }

        //[HttpDelete("{userId}/{fcmtoken}")]
        //public async Task<IActionResult> DeleteToken([FromRoute] string userId, [FromRoute] string fcmtoken)
        //{
        //    bool result = await _firebaseStorageService.DeleteTokenAsync(userId, fcmtoken);
        //    if (result)
        //        return Ok("Token đã được xóa thành công!");
        //    else
        //        return BadRequest("Không tìm thấy token để xóa.");
        //}
        //[HttpPost]
        //[Route("image")]
        //public async Task<IActionResult> UploadImage([FromForm] IFormFile image)
        //{
        //    if (image == null || image.Length == 0)
        //    {
        //        return BadRequest("No image file provided.");
        //    }

        //    var imageUrl = await _firebaseStorageService.UploadImageAsync(image);
        //    return Ok(new { imageUrl });
        //}
    }
}
