using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.RequestModel.UserManagement;

namespace TerrariumGardenTech.Service.IService
{
    public interface IAccountService
    {
        Task<(int, string)> CreateAccountAsync(AccountCreateRequest request);
        // Get all accounts 
        Task<(int, string, List<User>)> GetAllAccountsAsync();
        // Get by role
        Task<(int, string, List<User>)> GetAccountsByRoleAsync(string role, int page, int pageSize);
        // change account status
        Task<(int, string)> ChangeAccountStatusAsync(int userId, string status);

        Task<(int, string, List<User>)> GetAccountsAsync(int page, int pageSize);
        Task<(int, string, User)> GetAccountByIdAsync(int userId);
        Task<(int, string)> UpdateAccountAsync(int userId, AccountUpdateRequest request);
        Task<(int, string)> DeleteAccountAsync(int userId);

    }
}
