using TerrariumGardenTech.Common.RequestModel.Auth;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IUserService
{
    
    Task<(int, string)> RegisterUserAsync(UserRegisterRequest userRequest);
    Task<(int, string, string, string)> LoginAsync(string username, string password);
    Task<(int, string)> SendPasswordResetTokenAsync(string email);
    Task<(int, string)> ResetPasswordAsync(string token, string newPassword);
    Task<(int, string)> VerifyOtpAsync(string email, string otp);
    Task<(int Code, string Msg)> ResendOtpAsync(string email);
    Task<(int, string, string)> RefreshTokenAsync(string refreshToken);
    Task<IBusinessResult> GoogleLoginAsync(string accessToken);
    Task<User?> GetUserByUserIdAsync(int userId);
}