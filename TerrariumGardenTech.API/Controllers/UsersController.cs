using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using TerrariumGardenTech.Common;
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

        // tạm cmt lại phần đăng ký user để tránh lỗi không cần thiết
        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] UserRegisterRequest userRequest)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var (code, message) = await _userService.RegisterUserAsync(userRequest);
        //    if (code != Const.SUCCESS_CREATE_CODE)
        //    {
        //        if (code == Const.FAIL_CREATE_CODE)
        //            return Conflict(new { message });
        //        return BadRequest(new { message });
        //    }

        //    return Ok(new { message });
        //}

        // API đăng ký gửi OTP
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

        // API xác thực OTP
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest verifyOtpRequest)
        {
            var (code, message) = await _userService.VerifyOtpAsync(verifyOtpRequest.Email, verifyOtpRequest.Otp);
            if (code != Const.SUCCESS_CREATE_CODE)
            {
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

        // API yêu cầu user đã đăng nhập (tất cả role)
        [Authorize(Roles = "User,Staff,Manager,Admin")]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value ?? "Unknown";
            return Ok(new { message = $"Hello {username}, đây là profile của bạn." });
        }

        // API chỉ cho phép Admin truy cập
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-data")]
        public IActionResult GetAdminData()
        {
            return Ok(new { message = "Dữ liệu dành riêng cho Admin." });
        }

        // API cho phép Manager hoặc Admin truy cập
        [Authorize(Roles = "Manager,Admin")]
        [HttpGet("manage-data")]
        public IActionResult GetManageData()
        {
            return Ok(new { message = "Dữ liệu dành cho Manager hoặc Admin." });
        }

        // API cho phép Staff, Manager, Admin truy cập
        [Authorize(Roles = "Staff,Manager,Admin")]
        [HttpGet("staff-data")]
        public IActionResult GetStaffData()
        {
            return Ok(new { message = "Dữ liệu dành cho Staff, Manager hoặc Admin." });
        }
    }
}
