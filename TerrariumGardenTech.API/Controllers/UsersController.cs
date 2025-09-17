using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Auth;
using TerrariumGardenTech.Common.RequestModel.UserManagement;
using TerrariumGardenTech.Common.RequestModel.Voucher;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Service;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
        _logger = _logger; // Ensure this is initialized properly
    }


    [HttpGet("getbyuserid")]
    public async Task<IActionResult> GetUserByUserId(int userId)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _userService.GetUserByUserIdAsync(userId));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest userRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

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
        if (code != Const.SUCCESS_CREATE_CODE) return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest req)
    {
        var (code, msg) = await _userService.ResendOtpAsync(req.Email);
        if (code == Const.SUCCESS_CREATE_CODE) return Ok(new { message = msg });
        if (code == Const.ERROR_EXCEPTION) return StatusCode(500, new { message = msg });
        return BadRequest(new { message = msg });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var (code, message, token, refreshToken) =
            await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);

        if (code != Const.SUCCESS_READ_CODE || string.IsNullOrEmpty(token)) return Unauthorized(new { message });

        return Ok(new { token, refreshToken });
    }

    // API làm mới token
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var (code, message, token) = await _userService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);

        if (code != Const.SUCCESS_READ_CODE || string.IsNullOrEmpty(token)) return Unauthorized(new { message });

        return Ok(new { token });
    }


    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var (code, message) = await _userService.SendPasswordResetTokenAsync(request.Email);
        if (code != Const.SUCCESS_CREATE_CODE) return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest(new { message = "Mật khẩu xác nhận không khớp." });

        var (code, message) = await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
        if (code != Const.SUCCESS_CREATE_CODE) return BadRequest(new { message });

        return Ok(new { message });
    }

    // API đăng nhập với Google
    [HttpPost("login-google")]
    public async Task<IBusinessResult> LoginWithGoogle([FromBody] GoogleLoginRequest googleLoginRequest)
    {
        if (string.IsNullOrEmpty(googleLoginRequest.AccessToken))
            return new BusinessResult(Const.FAIL_READ_CODE, "Access token không hợp lệ", null);

        var result = await _userService.GoogleLoginAsync(googleLoginRequest.AccessToken);
        if (result.Status == Const.SUCCESS_READ_CODE)
            return new BusinessResult(result.Status, result.Message, result.Data);

        return new BusinessResult(result.Status, result.Message, null);
    }


    [Authorize(Roles = "User,Staff,Manager,Admin")]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        try
        {
            // Lấy giá trị username từ claim UniqueName (JwtRegisteredClaimNames.UniqueName)
            var username = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;

            // Nếu không tìm thấy claim UniqueName, thử lấy từ Name hoặc NameIdentifier
            if (string.IsNullOrEmpty(username))
                username = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value
                           ?? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                           ?? User.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                           ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                           ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Không tìm thấy tên người dùng trong token." });

            return Ok(new { message = $"Hello {username}, đây là profile của bạn." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin profile người dùng.");
            return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống." });
        }
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