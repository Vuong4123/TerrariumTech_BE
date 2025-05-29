using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.RequestModel.Auth;

namespace TerrariumGardenTech.Service.IService
{
    public interface IUserService
    {
        Task<(int, string)> RegisterUserAsync(UserRegisterRequest userRequest);
        Task<(int, string, string)> LoginAsync(string username, string password);
        Task<(int, string)> SendPasswordResetTokenAsync(string email);
        Task<(int, string)> ResetPasswordAsync(string token, string newPassword);
    }

}
