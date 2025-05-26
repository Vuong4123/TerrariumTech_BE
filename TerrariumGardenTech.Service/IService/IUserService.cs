using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.DTOs;

namespace TerrariumGardenTech.Service.IService
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(UserRegisterDto userDto);
        Task<string> LoginAsync(string username, string password);
    }
}
