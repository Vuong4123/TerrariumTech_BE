using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Enums;
using TerrariumGardenTech.Repositories.Repositories;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class VoucherService : IVoucherService
    {
        private readonly UnitOfWork _unitOfWork;

        public VoucherService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Kiểm tra tính hợp lệ của Voucher
        public async Task<bool> IsVoucherValidAsync(string code)
        {
            var voucher = await _unitOfWork.VoucherRepository.GetVoucherByCodeAsync(code);
            if (voucher == null) return false;

            var currentDate = DateTime.Now;

            // Kiểm tra Voucher có trong khoảng thời gian hợp lệ và trạng thái còn hoạt động
            return voucher.Status == VoucherStatus.Active.ToString() &&
                   voucher.ValidFrom <= currentDate &&
                   voucher.ValidTo >= currentDate;
        }

        public async Task<Voucher> GetVoucherByCodeAsync(string code)
        {
            return await _unitOfWork.VoucherRepository.GetVoucherByCodeAsync(code);
        }

        public async Task AddVoucherAsync(Voucher voucher)
        {
            await _unitOfWork.VoucherRepository.CreateAsync(voucher);
        }

        public async Task UpdateVoucherAsync(Voucher voucher)
        {
            await _unitOfWork.VoucherRepository.UpdateVoucherAsync(voucher);
        }

        public async Task DeleteVoucherAsync(int voucherId)
        {
            await _unitOfWork.VoucherRepository.DeleteVoucherAsync(voucherId);
        }
    }
}
