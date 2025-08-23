using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface IWalletServices
    {
        Task<IBusinessResult> DepositAsync(int userId, decimal amount, string method, int? orderId = null);
        Task<IBusinessResult> PayAsync(int userId, decimal amount, int orderId);
        Task<IBusinessResult> RefundAsync(int userId, decimal amount, int orderId);
        Task<decimal> GetBalanceAsync(int userId);
        Task<Wallet> GetOrCreateUserWallet(int userId);
    }
}
