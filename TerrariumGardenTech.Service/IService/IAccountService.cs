using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.UserManagement;

namespace TerrariumGardenTech.Service.IService
{
    public interface IAccountService
    {
        Task<IBusinessResult> CreateAccountAsync(AccountCreateRequest request);

        // Get all accounts 
        Task<IBusinessResult> GetAllAccountsAsync();

        // Get by role (with paging)
        Task<IBusinessResult> GetAccountsByRoleAsync(string role, int page, int pageSize);

        // Change account status
        Task<IBusinessResult> ChangeAccountStatusAsync(int userId, string status);

        // Get users with role: User | Admin (with paging)
        Task<IBusinessResult> GetAccountsAsync(int page, int pageSize);

        // Get by Id
        Task<IBusinessResult> GetAccountByIdAsync(int userId);

        // Update account
        Task<IBusinessResult> UpdateAccountAsync(int userId, AccountUpdateRequest request);

        // Delete account
        Task<IBusinessResult> DeleteAccountAsync(int userId);
    }
}
