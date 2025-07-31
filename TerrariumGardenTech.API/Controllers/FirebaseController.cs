using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.Firebase;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

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
            return BadRequest("UserId và Token không được để trống.");

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

    [HttpDelete("{userId}/{fcmtoken}")]
    public async Task<IActionResult> DeleteToken([FromRoute] string userId, [FromRoute] string fcmtoken)
    {
        var result = await _firebaseStorageService.DeleteTokenAsync(userId, fcmtoken);
        if (result)
            return Ok("Token đã được xóa thành công!");
        return BadRequest("Không tìm thấy token để xóa.");
    }
}