namespace TerrariumGardenTech.Common.ResponseModel.Wallet;

public class WalletBalanceHistoryDto
{
    public int WalletId { get; set; }
    public int UserId { get; set; }
    public decimal CurrentBalance { get; set; }
    public string WalletType { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<WalletTransactionDto> Transactions { get; set; } = new();
    public WalletStatistics Statistics { get; set; }
}

public class WalletTransactionDto
{
    public int TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? OrderId { get; set; }
    public decimal RunningBalance { get; set; } // Số dư sau giao dịch này
    public string Description { get; set; }
}

public class WalletStatistics
{
    public decimal TotalIncome { get; set; } // Tổng tiền vào
    public decimal TotalExpense { get; set; } // Tổng tiền ra
    public decimal NetChange { get; set; } // Thay đổi ròng
    public int TotalTransactions { get; set; }
    public decimal HighestTransaction { get; set; }
    public decimal LowestTransaction { get; set; }
}