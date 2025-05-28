using BCrypt.Net;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Auth;

namespace TerrariumGardenTech.Service.Service
{
    public class UserService : IUserService
    {
        private readonly GenericRepository<User> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly SmtpSettings _smtpSettings;

        public UserService(GenericRepository<User> userRepository, IConfiguration configuration, IOptions<SmtpSettings> smtpOptions)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _smtpSettings = smtpOptions.Value;
        }

        public async Task<(int, string)> RegisterUserAsync(UserRegisterRequest userRequest)
        {
            try
            {
                var existingUser = await _userRepository.FindOneAsync(u => u.Username == userRequest.Username || u.Email == userRequest.Email, false);
                if (existingUser != null)
                    return (Const.FAIL_CREATE_CODE, "Username hoặc Email đã tồn tại");

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRequest.Password);

                var newUser = new User
                {
                    Username = userRequest.Username,
                    Password = hashedPassword,
                    Email = userRequest.Email,
                    FullName = userRequest.FullName,
                    PhoneNumber = userRequest.PhoneNumber,
                    DateOfBirth = userRequest.DateOfBirth,
                    Gender = userRequest.Gender,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active",
                    RoleId = 1  
                };

                await _userRepository.CreateAsync(newUser);
                return (Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG);
            }
            catch (Exception)
            {
                return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại");
            }
        }


        public async Task<(int, string, string)> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.FindOneAsync(u => u.Username == username, false);
                if (user == null)
                    return (Const.FAIL_READ_CODE, "Tên đăng nhập không tồn tại", null);

                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                    return (Const.FAIL_READ_CODE, "Mật khẩu không đúng", null);

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings.GetValue<string>("SecretKey");
                var issuer = jwtSettings.GetValue<string>("Issuer");
                var audience = jwtSettings.GetValue<string>("Audience");
                var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes");

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
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
            catch (Exception)
            {
                return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại", null);
            }
        }

        public async Task<(int, string)> SendPasswordResetTokenAsync(string email)
        {
            try
            {
                var user = await _userRepository.FindOneAsync(u => u.Email == email, false);
                if (user == null)
                    return (Const.FAIL_READ_CODE, "Email không tồn tại");

                var resetToken = Guid.NewGuid().ToString();

                user.Token = resetToken;
                user.StartToken = DateTime.UtcNow;
                user.EndToken = DateTime.UtcNow.AddHours(1);

                await _userRepository.UpdateAsync(user);

                var resetLink = $"https://your-frontend-domain/reset-password?token={resetToken}";

                var subject = "Đặt lại mật khẩu";
                var body = $@"
                    <p>Xin chào {user.FullName},</p>
                    <p>Bạn vừa yêu cầu đặt lại mật khẩu. Vui lòng bấm vào link bên dưới để đặt lại mật khẩu mới:</p>
                    <p><a href='{resetLink}'>Đặt lại mật khẩu</a></p>
                    <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
                    <p>Trân trọng,<br/>TerrariumGardenTech Team</p>";

                await SendEmailAsync(user.Email, subject, body);

                return (Const.SUCCESS_CREATE_CODE, "Email gửi thành công");
            }
            catch (Exception)
            {
                return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại");
            }
        }

        public async Task<(int, string)> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                var user = await _userRepository.FindOneAsync(u => u.Token == token, false);
                if (user == null)
                    return (Const.FAIL_READ_CODE, "Token không hợp lệ");

                if (!user.EndToken.HasValue || user.EndToken.Value < DateTime.UtcNow)
                    return (Const.FAIL_READ_CODE, "Token đã hết hạn");

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.Token = null;
                user.StartToken = null;
                user.EndToken = null;

                await _userRepository.UpdateAsync(user);
                return (Const.SUCCESS_CREATE_CODE, "Đổi mật khẩu thành công");
            }
            catch (Exception)
            {
                return (Const.ERROR_EXCEPTION, "Lỗi hệ thống, vui lòng thử lại");
            }
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
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
