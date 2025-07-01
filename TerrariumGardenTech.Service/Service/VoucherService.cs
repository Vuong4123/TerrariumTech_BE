using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Enums;
using TerrariumGardenTech.Repositories.Repositories;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class VoucherService : IVoucherService
    {
        private readonly VoucherRepository _voucherRepository;

        public VoucherService(VoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        // Kiểm tra tính hợp lệ của Voucher
        public async Task<bool> IsVoucherValidAsync(string code)
        {
            var voucher = await _voucherRepository.GetVoucherByCodeAsync(code);
            if (voucher == null) return false;

            var currentDate = DateTime.Now;

            // Kiểm tra Voucher có trong khoảng thời gian hợp lệ và trạng thái còn hoạt động
            return voucher.Status == VoucherStatus.Active.ToString() &&
                   voucher.ValidFrom <= currentDate &&
                   voucher.ValidTo >= currentDate;
        }

        public async Task<Voucher> GetVoucherByCodeAsync(string code)
        {
            return await _voucherRepository.GetVoucherByCodeAsync(code);
        }

        public async Task AddVoucherAsync(Voucher voucher)
        {
            await _voucherRepository.CreateAsync(voucher);
        }

        public async Task UpdateVoucherAsync(Voucher voucher)
        {
            await _voucherRepository.UpdateVoucherAsync(voucher);
        }

        public async Task DeleteVoucherAsync(int voucherId)
        {
            await _voucherRepository.DeleteVoucherAsync(voucherId);
        }
    }
}
