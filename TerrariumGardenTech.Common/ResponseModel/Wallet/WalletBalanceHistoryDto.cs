namespace TerrariumGardenTech.Common.ResponseModel.Wallet;


public class WalletStatistics
{
    public decimal TotalIncome { get; set; } // Tổng tiền vào
    public decimal TotalExpense { get; set; } // Tổng tiền ra
    public decimal NetChange { get; set; } // Thay đổi ròng
    public int TotalTransactions { get; set; }
    public decimal HighestTransaction { get; set; }
    public decimal LowestTransaction { get; set; }
}
// ✅ USER WALLET HISTORY RESPONSE
public class WalletBalanceHistoryDto
{
    public int WalletId { get; set; }
    public int UserId { get; set; }
    public decimal CurrentBalance { get; set; }
    public string WalletType { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<WalletTransactionDto> Transactions { get; set; } = new List<WalletTransactionDto>();
    public WalletStatisticsDto Statistics { get; set; } // ✅ ĐÂY NÈ
}

// ✅ USER WALLET TRANSACTION
public class WalletTransactionDto
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? OrderId { get; set; }
    public decimal RunningBalance { get; set; }
    public string Description { get; set; }
}

// ✅ USER WALLET STATISTICS
public class WalletStatisticsDto
{
    public decimal TotalIncome { get; set; }        // ✅ Tổng thu nhập
    public decimal TotalExpense { get; set; }       // ✅ Tổng chi tiêu  
    public decimal NetChange { get; set; }          // ✅ Thay đổi ròng
    public int TotalTransactions { get; set; }      // ✅ Tổng giao dịch
    public decimal HighestTransaction { get; set; } // ✅ Giao dịch cao nhất
    public decimal LowestTransaction { get; set; }  // ✅ Giao dịch thấp nhất
}


// ✅ ADMIN ALL WALLET HISTORY RESPONSE
public class AdminAllWalletHistoryDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalWallets { get; set; }
    public decimal SystemTotalBalance { get; set; }
    public List<AdminWalletTransactionDto> AllTransactions { get; set; } = new List<AdminWalletTransactionDto>();
    public AdminStatisticsDto Statistics { get; set; }
}

// ✅ ADMIN WALLET TRANSACTION
public class AdminWalletTransactionDto
{
    public int TransactionId { get; set; }
    public int WalletId { get; set; }
    public int UserId { get; set; }
    public string UserWalletType { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? OrderId { get; set; }
    public string Description { get; set; }
}

// ✅ ADMIN STATISTICS
public class AdminStatisticsDto
{
    public int TotalSystemTransactions { get; set; }
    public decimal TotalSystemDeposits { get; set; }
    public decimal TotalSystemWithdrawals { get; set; }
    public int TotalDepositTransactions { get; set; }
    public int TotalWithdrawalTransactions { get; set; }
    public int ActiveWalletsWithTransactions { get; set; }
    public decimal SystemNetFlow { get; set; }
    public decimal AverageTransactionAmount { get; set; }

    // ✅ ADDITIONAL ADMIN METRICS
    public decimal LargestDeposit { get; set; }
    public decimal LargestWithdrawal { get; set; }
    public DateTime? MostRecentTransaction { get; set; }
}

// ✅ WALLET SUMMARY
public class WalletSummaryDto
{
    public int WalletId { get; set; }
    public int UserId { get; set; }
    public decimal Balance { get; set; }
    public string WalletType { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalDeposited { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastActivity { get; set; }
}
