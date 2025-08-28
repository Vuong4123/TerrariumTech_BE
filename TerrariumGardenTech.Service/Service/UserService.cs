using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Auth;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Common.Security;

namespace TerrariumGardenTech.Service.Service;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;
    private readonly SmtpSettings _smtpSettings;
    private readonly UnitOfWork _unitOfWork;
    private readonly ICloudinaryService _cloudinary;


    public UserService(UnitOfWork unitOfWork, IConfiguration configuration, IOptions<SmtpSettings> smtpOptions,
        ILogger<UserService> logger, ICloudinaryService cloudinary)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _smtpSettings = smtpOptions.Value;
        _logger = logger;
        _cloudinary = cloudinary;
    }



    public async Task<(int, string)> RegisterUserAsync(UserRegisterRequest userRequest)
    {
        try
        {
            var existingUser =
                await _unitOfWork.User.FindOneAsync(
                    u => u.Username == userRequest.Username || u.Email == userRequest.Email, false);
            if (existingUser != null)
                return (Const.FAIL_CREATE_CODE, "Username hoặc Email đã tồn tại");

            // ✅ THÊM: kiểm tra độ mạnh mật khẩu
            var (ok, err) = PasswordValidator.Validate(userRequest.PasswordHash);
            if (!ok)
                return (Const.FAIL_CREATE_CODE, err);

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRequest.PasswordHash);

            var newUser = new User
            {
                Username = userRequest.Username,
                PasswordHash = hashedPassword,
                Email = userRequest.Email,
                FullName = userRequest.FullName,
                PhoneNumber = userRequest.PhoneNumber,
                DateOfBirth = userRequest.DateOfBirth,
                Gender = userRequest.Gender,
                CreatedAt = DateTime.UtcNow,
                Status = AccountStatus.Inactive.ToString(), // Sử dụng AccountStatus.Inactive thay vì chuỗi "Active"
                RoleId = (int)RoleStatus.User // Mặc định là User, sử dụng RoleStatus.User enum
            };

            // Tạo OTP và gửi email
            var otp = GenerateOtp();
            await SendOtpEmailAsync(userRequest.Email, otp);

            // Lưu OTP vào cơ sở dữ liệu
            newUser.Otp = otp;
            newUser.OtpExpiration = DateTime.UtcNow.AddMinutes(10); // OTP hết hạn trong 10 phút
            await _unitOfWork.User.CreateAsync(newUser);

            return (Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo tài khoản");
            return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại");
        }
    }


    public async Task<(int, string)> VerifyOtpAsync(string email, string otp)
    {
        try
        {
            var user = await _unitOfWork.User.FindOneAsync(u => u.Email == email, false);
            if (user == null) return (Const.FAIL_READ_CODE, "Email không tồn tại");

            // Kiểm tra OTP và thời gian hết hạn
            if (user.Otp == null || user.Otp != otp) return (Const.FAIL_READ_CODE, "Mã OTP không đúng");

            if (user.OtpExpiration == null || user.OtpExpiration < DateTime.UtcNow)
                return (Const.FAIL_READ_CODE, "Mã OTP đã hết hạn");

            user.Status = AccountStatus.Active.ToString(); // Kích hoạt tài khoản sau khi xác thực OTP
            await _unitOfWork.User.UpdateAsync(user); // Đảm bảo cập nhật thông tin trạng thái tài khoản

            return (Const.SUCCESS_CREATE_CODE, "OTP xác thực thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xác thực OTP");
            return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại");
        }
    }


    public async Task<(int, string, string, string)> LoginAsync(string username, string password)
    {
        try
        {
            var user = await _unitOfWork.User.Context().Users
                .Include(u => u.Role)
                .Include(m => m.Memberships)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return (Const.FAIL_READ_CODE, "Tên đăng nhập không tồn tại", null, null);

            if (user.PasswordHash == null)
                return (Const.FAIL_READ_CODE, "Mật khẩu không đúng", null, null);

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (Const.FAIL_READ_CODE, "Mật khẩu không đúng", null, null);

            // Tạo JWT Token
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");
            var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes");
            var isMembership = user.Memberships.Any(m => m.Status.Equals(CommonData.AccountStatus.Active) && m.EndDate > DateTime.UtcNow);
            var personalize = _unitOfWork.Personalize.GetByUserIdAsync(user.UserId).Result;
            string checkIspersional = personalize != null ? "true" : "false";
            bool isOtp = user.Otp != null;
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role,
                    user.Role?.RoleName ?? RoleStatus.User.ToString()), // Sử dụng enum RoleStatus.User
                new Claim("email", user.Email ?? ""),
                new Claim("fullName", user.FullName ?? ""),
                new Claim("phoneNumber", user.PhoneNumber ?? ""),
                new Claim("gender", user.Gender ?? ""),
                new Claim("isMembership", isMembership.ToString()),
                new Claim("isPersonalize", checkIspersional),
                new Claim("isOtp", isOtp.ToString()),
                new Claim("status", user.Status ?? AccountStatus.Active.ToString()) // Sử dụng enum AccountStatus.Active
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Tạo refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7); // Refresh token hết hạn sau 7 ngày

            await _unitOfWork.User.UpdateAsync(user);

            return (Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, jwtToken, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập");
            return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại", null, null);
        }
    }


    public async Task<(int, string, string)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = await _unitOfWork.User.Context().Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || !user.RefreshTokenExpiryDate.HasValue ||
                user.RefreshTokenExpiryDate.Value <= DateTime.UtcNow)
                return (Const.FAIL_READ_CODE, "Refresh token không hợp lệ hoặc đã hết hạn", null);

            // Tạo lại JWT token
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");
            var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role,
                    user.Role?.RoleName ?? RoleStatus.User.ToString()), // Sử dụng RoleStatus.User thay vì chuỗi "User"
                new Claim("email", user.Email ?? ""),
                new Claim("fullName", user.FullName ?? ""),
                new Claim("phoneNumber", user.PhoneNumber ?? ""),
                new Claim("gender", user.Gender ?? ""),
                new Claim("status",
                    user.Status ??
                    AccountStatus.Active.ToString()) // Sử dụng AccountStatus.Active thay vì chuỗi "Active"
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, jwtToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi làm mới token");
            return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại", null);
        }
    }


    public async Task<(int, string)> SendPasswordResetTokenAsync(string email)
    {
        try
        {
            var user = await _unitOfWork.User.FindOneAsync(u => u.Email == email, false);
            if (user == null)
                return (Const.FAIL_READ_CODE, "Email không tồn tại");

            // Tạo reset token và thiết lập thời gian hết hạn
            var resetToken = Guid.NewGuid().ToString();

            user.Token = resetToken;
            user.StartToken = DateTime.UtcNow;
            user.EndToken = DateTime.UtcNow.AddHours(1); // Token hết hạn sau 1 giờ

            await _unitOfWork.User.UpdateAsync(user);

            // Tạo liên kết đặt lại mật khẩu
            var resetLink = $"https://terra-tech-garden.vercel.app/reset-password?token={resetToken}";

            // Cấu hình email
            var subject = "Đặt lại mật khẩu";
            var body = $@"
                    <p>Xin chào {user.FullName},</p>
                    <p>Bạn vừa yêu cầu đặt lại mật khẩu. Vui lòng bấm vào link bên dưới để đặt lại mật khẩu mới:</p>
                    <p><a href='{resetLink}'>Đặt lại mật khẩu</a></p>
                    <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
                    <p>Trân trọng,<br/>TerrariumGardenTech Team</p>";

            // Gửi email
            await SendEmailAsync(user.Email, subject, body);

            return (Const.SUCCESS_CREATE_CODE, "Email gửi thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email reset mật khẩu");
            return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại");
        }
    }

    public async Task<(int, string)> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var user = await _unitOfWork.User.FindOneAsync(u => u.Token == token, false);
            if (user == null)
                return (Const.FAIL_READ_CODE, "Token không hợp lệ");

            // Kiểm tra token hết hạn
            if (!user.EndToken.HasValue || user.EndToken.Value < DateTime.UtcNow)
                return (Const.FAIL_READ_CODE, "Token đã hết hạn");

            // ✅ THÊM: kiểm tra độ mạnh mật khẩu mới
            var (ok, err) = PasswordValidator.Validate(newPassword);
            if (!ok)
                return (Const.FAIL_CREATE_CODE, err);

            // Mã hóa mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.Token = null; // Xóa token sau khi reset
            user.StartToken = null;
            user.EndToken = null;

            // Cập nhật thông tin người dùng
            await _unitOfWork.User.UpdateAsync(user);
            return (Const.SUCCESS_CREATE_CODE, "Đổi mật khẩu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đổi mật khẩu");
            return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại");
        }
    }

    public async Task<IBusinessResult> GoogleLoginAsync(string accessToken)
    {
        try
        {
            // Lấy thông tin người dùng từ Google API
            if (string.IsNullOrEmpty(accessToken))
                return new BusinessResult(Const.ERROR_EXCEPTION_CODE_LOGINGOOGLE, Const.ERROR_EXCEPTION_MSG_LOGINGOOGLE,null);    
            // Giả sử `response` chứa JSON của người dùng, bạn có thể parse nó và lấy các thông tin cần thiết          
            var userInfo = await VerifyGoogleAccessToken(accessToken);

            // Kiểm tra người dùng có trong hệ thống chưa
            var existingUser = await _unitOfWork.User.FindOneAsync(u => u.Email == userInfo.Email, false);
            if (existingUser == null)
            {
                // Nếu người dùng chưa tồn tại, tạo mới người dùng
                var newUser = new User
                {
                    Username = userInfo.Email.Split('@')[0], // Sử dụng email làm username
                    Email = userInfo.Email,
                    FullName = userInfo.Name,
                    PasswordHash = "123456789", // Mật khẩu tạm thời khi login bằng gg, có thể thay đổi sau
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active",
                    RoleId = 1 // Mặc định role User
                };

                await _unitOfWork.User.CreateAsync(newUser);
                existingUser = newUser;
            }

            // Tạo JWT cho người dùng
            var token = GenerateJwtToken(existingUser);

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Đăng nhập Google thành công", token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập Google");
            return new BusinessResult(Const.FAIL_LOGIN_CODE, Const.FAIL_LOGIN_MSG);
        }
    }


    private string GenerateOtp()
    {
        var random = new Random();
        var otp = random.Next(100000, 999999); // Tạo OTP gồm 6 chữ số
        return otp.ToString();
    }

    private async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("TerrariumGardenTech", _smtpSettings.Username));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Mã OTP để xác thực tài khoản";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                    <p>Xin chào,</p>
                    <p>Bạn vừa đăng ký tài khoản tại TerrariumGardenTech. Mã OTP của bạn là:</p>
                    <h3>{otp}</h3>
                    <p>Mã OTP sẽ hết hạn sau 1 phút.</p>
                    <p>Trân trọng,<br/>TerrariumGardenTech Team</p>"
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }


    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }


    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("TerrariumGardenTech", _smtpSettings.Username));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private async Task<Userinfo> VerifyGoogleAccessToken(string accessToken)
    {
        var oauthService = new Oauth2Service(new BaseClientService.Initializer
        {
            HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken)
            // ApplicationName = "main"
        });

        // Lấy thông tin user từ Google
        var userInfo = await oauthService.Userinfo.Get().ExecuteAsync();

        return userInfo; // Trả về object có sẵn từ thư viện
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings.GetValue<string>("SecretKey");
        var issuer = jwtSettings.GetValue<string>("Issuer");
        var audience = jwtSettings.GetValue<string>("Audience");
        var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? RoleStatus.User.ToString()),
            new Claim("email", user.Email ?? ""),
            new Claim("fullName", user.FullName ?? ""),
            new Claim("phoneNumber", user.PhoneNumber ?? ""),
            new Claim("gender", user.Gender ?? ""),
            new Claim("status", user.Status ?? AccountStatus.Active.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(int Code, string Msg)> ResendOtpAsync(string email)
    {
        try
        {
            var user = await _unitOfWork.User.FindOneAsync(u => u.Email == email, false);
            if (user == null)
                return (Const.FAIL_READ_CODE, "Email không tồn tại");

            // Chỉ cho resend khi tài khoản CHƯA kích hoạt
            if (string.Equals(user.Status, AccountStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                return (Const.FAIL_CREATE_CODE, "Tài khoản đã kích hoạt, không cần OTP");

            // Cooldown: 60 giây giữa 2 lần gửi
            var now = DateTime.UtcNow;
            if (user.OtpSentAt.HasValue && now - user.OtpSentAt.Value < TimeSpan.FromSeconds(60))
            {
                var wait = 60 - (int)(now - user.OtpSentAt.Value).TotalSeconds;
                return (Const.FAIL_CREATE_CODE, $"Vui lòng thử lại sau {wait}s");
            }

            // Giới hạn số lần resend trong 1 giờ (ví dụ 3 lần)
            // Reset cửa sổ sau 1 giờ
            if (user.OtpSentAt.HasValue && now - user.OtpSentAt.Value > TimeSpan.FromHours(1))
            {
                user.OtpResendCount = 0;
            }
            if (user.OtpResendCount >= 3)
                return (Const.FAIL_CREATE_CODE, "Bạn đã vượt quá số lần gửi lại OTP. Vui lòng thử lại sau 1 giờ.");

            // Tạo và lưu OTP mới
            var otp = GenerateOtp();
            user.Otp = otp;
            user.OtpExpiration = now.AddMinutes(10);   // hoặc 1 phút nếu bạn muốn ngắn
            user.OtpSentAt = now;
            user.OtpResendCount += 1;

            await _unitOfWork.User.UpdateAsync(user);

            // Gửi email OTP
            await SendOtpEmailAsync(email, otp);

            return (Const.SUCCESS_CREATE_CODE, "Đã gửi lại OTP");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi lại OTP");
            return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại");
        }
    }
}