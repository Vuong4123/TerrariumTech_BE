using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Auth;

namespace TerrariumGardenTech.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (code, message) = await _userService.RegisterUserAsync(userRequest);
            if (code != Const.SUCCESS_CREATE_CODE)
            {
                if (code == Const.FAIL_CREATE_CODE)
                    return Conflict(new { message });
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var (code, message, token) = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
            if (code != Const.SUCCESS_READ_CODE || string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message });
            }

            return Ok(new { token });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var (code, message) = await _userService.SendPasswordResetTokenAsync(request.Email);
            if (code != Const.SUCCESS_CREATE_CODE)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Mật khẩu xác nhận không khớp." });
            }

            var (code, message) = await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (code != Const.SUCCESS_CREATE_CODE)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }
    }

}
