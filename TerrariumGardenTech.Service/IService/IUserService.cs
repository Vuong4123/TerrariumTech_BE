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
        Task<bool> RegisterUserAsync(UserRegisterRequest userRequest);
        Task<string> LoginAsync(string username, string password);
    }
}
