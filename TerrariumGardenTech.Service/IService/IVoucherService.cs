using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.IService
{
    public interface IVoucherService
    {
        Task<bool> IsVoucherValidAsync(string code);
        Task<Voucher> GetVoucherByCodeAsync(string code);
        Task AddVoucherAsync(Voucher voucher);
        Task UpdateVoucherAsync(Voucher voucher);
        Task DeleteVoucherAsync(int voucherId);
    }
}
