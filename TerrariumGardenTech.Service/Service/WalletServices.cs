using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class WalletServices : IWalletServices
    {
        private readonly UnitOfWork _unitOfWork;
        public WalletServices(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Wallet> GetOrCreateUserWallet(int userId)
        {
            var wallet = await _unitOfWork.Wallet
                .FindOneAsync(w => w.UserId == userId && w.WalletType == "User");

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    WalletType = "User",
                    Balance = 0
                };
                await _unitOfWork.Wallet.CreateAsync(wallet);
                await _unitOfWork.SaveAsync();
            }
            return wallet;
        }
        public async Task<IBusinessResult> DepositAsync(int userId, decimal amount, string method, int? orderId = null)
        {
            var wallet = await GetOrCreateUserWallet(userId);
            wallet.Balance += amount;

            await _unitOfWork.WalletTransactionRepository.CreateAsync(new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                Type = "Deposit",
                CreatedDate = DateTime.UtcNow,
                OrderId = orderId

            });

            await _unitOfWork.SaveAsync();
            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Nạp tiền thành công", wallet.Balance);
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var wallet = await GetOrCreateUserWallet(userId);
            return wallet.Balance;
        }

        public async Task<IBusinessResult> PayAsync(int userId, decimal amount, int orderId)
        {
            var wallet = await GetOrCreateUserWallet(userId);
            if (wallet.Balance < amount)
                return new BusinessResult(Const.FAIL_READ_CODE, "Số dư không đủ");

            wallet.Balance -= amount;

            // Cộng vào ví doanh thu đang xử lý
            var processingWallet = await _unitOfWork.Wallet.FindOneAsync(w => w.WalletType == "ProcessingRevenue");
            if (processingWallet != null)
                processingWallet.Balance += amount;

            await _unitOfWork.WalletTransactionRepository.CreateAsync(new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = -amount,
                Type = "Payment",
                CreatedDate=DateTime.UtcNow,
                OrderId=orderId
               
            });

            await _unitOfWork.SaveAsync();
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Thanh toán thành công", wallet.Balance);
        }

        public async Task<IBusinessResult> RefundAsync(int userId, decimal amount, int orderId)
        {
            var wallet = await GetOrCreateUserWallet(userId);

            wallet.Balance += amount;

            // Trừ tiền ví ProcessingRevenue
            var processingWallet = await _unitOfWork.Wallet.FindOneAsync(w => w.WalletType == "ProcessingRevenue");
            if (processingWallet != null)
                processingWallet.Balance -= amount;

            await _unitOfWork.WalletTransactionRepository.CreateAsync(new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                Type = "Refund",
                CreatedDate = DateTime.UtcNow,
                OrderId = orderId

            });

            await _unitOfWork.SaveAsync();
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Hoàn tiền thành công", wallet.Balance);
        }
    }
}
