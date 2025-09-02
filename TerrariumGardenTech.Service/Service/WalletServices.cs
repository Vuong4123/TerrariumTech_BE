using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Common.ResponseModel.Wallet;
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
        public async Task<WalletBalanceHistoryDto> GetWalletBalanceHistoryAsync(
        int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var from = fromDate ?? DateTime.UtcNow.AddMonths(-3);
            var to = toDate ?? DateTime.UtcNow;

            // Lấy thông tin ví
            var wallet = await _unitOfWork.Wallet.GetByUserIdAsync(userId);
            if (wallet == null)
                throw new("Không tìm thấy ví của người dùng");

            // Lấy tất cả giao dịch để tính running balance
            var allTransactions = await _unitOfWork.Wallet
                .GetAllTransactionsByWalletIdAsync(wallet.WalletId);

            // Lấy giao dịch trong khoảng thời gian
            var filteredTransactions = await _unitOfWork.Wallet
                .GetTransactionsByWalletIdAndDateRangeAsync(wallet.WalletId, from, to);

            // Tính running balance cho từng giao dịch
            var transactionDtos = CalculateRunningBalance(allTransactions, filteredTransactions);

            // Tính thống kê
            var statistics = CalculateStatistics(filteredTransactions);

            return new WalletBalanceHistoryDto
            {
                WalletId = wallet.WalletId,
                UserId = wallet.UserId,
                CurrentBalance = wallet.Balance,
                WalletType = wallet.WalletType,
                FromDate = from,
                ToDate = to,
                Transactions = transactionDtos,
                Statistics = statistics
            };
        }
        // ✅ ADMIN METHOD - TẤT CẢ GIAO DỊCH CỦA TẤT CẢ USER
        public async Task<AdminAllWalletHistoryDto> GetAllWalletHistoryForAdminAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var from = fromDate ?? DateTime.UtcNow.AddMonths(-3);
            var to = toDate ?? DateTime.UtcNow;

            // ✅ LẤY TẤT CẢ GIAO DỊCH - KHÔNG FILTER GÌ CẢ (ngoại trừ date)
            var allSystemTransactions = await _unitOfWork.Wallet.GetAllTransactionsByWalletIdAndDateRangeAsync(from, to);

            // ✅ LẤY TẤT CẢ WALLET ĐỂ MAP USER INFO
            var allWallets = await _unitOfWork.Wallet.GetAllAsync();
            var walletDict = allWallets.ToDictionary(w => w.WalletId, w => w);

            // Convert to admin DTOs với thông tin user
            var adminTransactionDtos = allSystemTransactions.Select(t => new AdminWalletTransactionDto
            {
                TransactionId = t.TransactionId,
                WalletId = t.WalletId,
                UserId = walletDict.ContainsKey(t.WalletId) ? walletDict[t.WalletId].UserId : 0,
                UserWalletType = walletDict.ContainsKey(t.WalletId) ? walletDict[t.WalletId].WalletType : "Unknown",
                Amount = t.Amount,
                Type = t.Type,
                CreatedDate = t.CreatedDate,
                OrderId = t.OrderId,
                Description = GetTransactionDescription(t)
            }).OrderByDescending(t => t.CreatedDate).ToList();

            // Tính thống kê tổng thể
            var adminStatistics = CalculateAdminStatistics(allSystemTransactions);

            // Tóm tắt các ví
            var walletSummaries = allWallets.Select(w => new WalletSummaryDto
            {
                WalletId = w.WalletId,
                UserId = w.UserId,
                Balance = w.Balance,
                WalletType = w.WalletType,
                TransactionCount = allSystemTransactions.Count(t => t.WalletId == w.WalletId)
            }).ToList();

            return new AdminAllWalletHistoryDto
            {
                FromDate = from,
                ToDate = to,
                TotalTransactions = allSystemTransactions.Count,
                TotalWallets = allWallets.Count,
                SystemTotalBalance = allWallets.Sum(w => w.Balance),
                AllTransactions = adminTransactionDtos, // ✅ TẤT CẢ GIAO DỊCH
                Statistics = adminStatistics
            };
        }

        private AdminStatisticsDto CalculateAdminStatistics(List<WalletTransaction> allTransactions)
        {
            var deposits = allTransactions.Where(t => t.Amount > 0).ToList();
            var withdrawals = allTransactions.Where(t => t.Amount < 0).ToList();
            var uniqueWallets = allTransactions.Select(t => t.WalletId).Distinct().Count();

            return new AdminStatisticsDto
            {
                TotalSystemTransactions = allTransactions.Count,
                TotalSystemDeposits = deposits.Sum(t => t.Amount),
                TotalSystemWithdrawals = Math.Abs(withdrawals.Sum(t => t.Amount)),
                TotalDepositTransactions = deposits.Count,
                TotalWithdrawalTransactions = withdrawals.Count,
                ActiveWalletsWithTransactions = uniqueWallets,
                SystemNetFlow = deposits.Sum(t => t.Amount) + withdrawals.Sum(t => t.Amount),
                AverageTransactionAmount = allTransactions.Any() ? allTransactions.Average(t => Math.Abs(t.Amount)) : 0
            };
        }

        private string GetTransactionDescription(WalletTransaction transaction)
        {
            return transaction.Type switch
            {
                "Deposit" => "Nạp tiền",
                "Payment" => transaction.OrderId.HasValue ? $"Thanh toán đơn hàng #{transaction.OrderId}" : "Thanh toán",
                "MembershipPayment" => "Thanh toán membership",
                "Refund" => transaction.OrderId.HasValue ? $"Hoàn tiền đơn hàng #{transaction.OrderId}" : "Hoàn tiền",
                _ => transaction.Type
            };
        }

        private List<WalletTransactionDto> CalculateRunningBalance(
            List<WalletTransaction> allTransactions,
            List<WalletTransaction> filteredTransactions)
        {
            var result = new List<WalletTransactionDto>();
            decimal runningBalance = 0;

            // Tính running balance từ đầu đến cuối
            foreach (var transaction in allTransactions.OrderBy(t => t.CreatedDate))
            {
                // Cộng/trừ số dư dựa trên loại giao dịch
                if (IsIncomeTransaction(transaction.Type))
                {
                    runningBalance += transaction.Amount;
                }
                else
                {
                    runningBalance -= Math.Abs(transaction.Amount);
                }

                // Chỉ thêm vào kết quả nếu nằm trong khoảng thời gian lọc
                if (filteredTransactions.Any(f => f.TransactionId == transaction.TransactionId))
                {
                    result.Add(new WalletTransactionDto
                    {
                        TransactionId = transaction.TransactionId,
                        Amount = transaction.Amount,
                        Type = transaction.Type,
                        CreatedDate = transaction.CreatedDate,
                        OrderId = transaction.OrderId,
                        RunningBalance = runningBalance,
                        Description = GetTransactionDescription(transaction.Type, transaction.Amount)
                    });
                }
            }

            return result.OrderByDescending(t => t.CreatedDate).ToList();
        }

        private bool IsIncomeTransaction(string type)
        {
            // Định nghĩa các loại giao dịch là thu nhập
            return type switch
            {
                "DEPOSIT" => true,
                "REFUND" => true,
                "BONUS" => true,
                "CASHBACK" => true,
                _ => false
            };
        }

        private string GetTransactionDescription(string type, decimal amount)
        {
            return type switch
            {
                "DEPOSIT" => $"Nạp tiền vào ví +{amount:N0}",
                "PAYMENT" => $"Thanh toán đơn hàng -{Math.Abs(amount):N0}",
                "REFUND" => $"Hoàn tiền +{amount:N0}",
                "WITHDRAW" => $"Rút tiền -{Math.Abs(amount):N0}",
                "BONUS" => $"Tiền thưởng +{amount:N0}",
                "CASHBACK" => $"Hoàn tiền mua hàng +{amount:N0}",
                _ => $"Giao dịch {type} {amount:N0}"
            };
        }

        private WalletStatisticsDto CalculateStatistics(List<WalletTransaction> transactions)
        {
            if (!transactions.Any())
            {
                return new WalletStatisticsDto();
            }

            var income = transactions
                .Where(t => IsIncomeTransaction(t.Type))
                .Sum(t => t.Amount);

            var expense = transactions
                .Where(t => !IsIncomeTransaction(t.Type))
                .Sum(t => Math.Abs(t.Amount));

            return new WalletStatisticsDto
            {
                TotalIncome = income,
                TotalExpense = expense,
                NetChange = income - expense,
                TotalTransactions = transactions.Count,
                HighestTransaction = transactions.Max(t => Math.Abs(t.Amount)),
                LowestTransaction = transactions.Min(t => Math.Abs(t.Amount))
            };
        }
        public async Task<IBusinessResult> RefundAsync(int userId, decimal amount, int orderId)
        {
            var wallet = await GetOrCreateUserWallet(userId);

            wallet.Balance += amount;

            // Trừ tiền ví ProcessingRevenue
            var processingWallet = await _unitOfWork.Wallet.FindOneAsync(w => w.WalletType == "User");
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
        public async Task<WalletPaymentResult> ProcessMembershipPaymentAsync(int userId, int orderId, decimal amount)
        {
            try
            {
                // Lấy hoặc tạo ví
                var wallet = await GetOrCreateWalletAsync(userId);

                // Kiểm tra số dư
                if (wallet.Balance < amount)
                {
                    return new WalletPaymentResult
                    {
                        Success = false,
                        Message = $"Số dư không đủ. Cần: {amount:N0}đ, Có: {wallet.Balance:N0}đ",
                        WalletInfo = null
                    };
                }

                decimal previousBalance = wallet.Balance;

                // Trừ tiền từ ví
                wallet.Balance -= amount;
                await _unitOfWork.Wallet.UpdateAsync(wallet);

                // Tạo transaction history
                var transaction = new WalletTransaction
                {
                    WalletId = wallet.WalletId,
                    Amount = -amount, // Số âm vì là trừ tiền
                    Type = "Payment",
                    CreatedDate = DateTime.UtcNow,
                    OrderId = orderId
                };
                await _unitOfWork.WalletTransactionRepository.CreateAsync(transaction);

                // Cập nhật trạng thái order thành paid
                var order = await _unitOfWork.Order.GetByIdAsync(orderId);
                if (order != null)
                {
                    order.PaymentStatus = "Paid";
                    order.TransactionId = transaction.TransactionId.ToString();
                    await _unitOfWork.Order.UpdateAsync(order);
                }

                await _unitOfWork.SaveAsync();

                return new WalletPaymentResult
                {
                    Success = true,
                    Message = "Thanh toán thành công",
                    WalletInfo = new WalletPaymentInfo
                    {
                        WalletId = wallet.WalletId,
                        PreviousBalance = previousBalance,
                        NewBalance = wallet.Balance,
                        AmountPaid = amount,
                        Status = "Paid"
                    }
                };
            }
            catch (Exception ex)
            {
                return new WalletPaymentResult
                {
                    Success = false,
                    Message = $"Lỗi xử lý thanh toán: {ex.Message}",
                    WalletInfo = null
                };
            }
        }

        public async Task<Wallet> GetOrCreateWalletAsync(int userId)
        {
            var wallet = await _unitOfWork.Wallet.GetByUserIdAndTypeAsync(userId, "User");
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    Balance = 0,
                    WalletType = "User"
                };
                await _unitOfWork.Wallet.CreateAsync(wallet);
                await _unitOfWork.SaveAsync();
            }
            return wallet;
        }

        public async Task<decimal> GetBalanceAsync2(int userId)
        {
            var wallet = await _unitOfWork.Wallet.GetByUserIdAndTypeAsync(userId, "User");
            return wallet?.Balance ?? 0;
        }

        public async Task<bool> HasSufficientBalanceAsync(int userId, decimal amount)
        {
            var balance = await GetBalanceAsync2(userId);
            return balance >= amount;
        }
    }
    public class WalletPaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public WalletPaymentInfo? WalletInfo { get; set; }
    }
}
