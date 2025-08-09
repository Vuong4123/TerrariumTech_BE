using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.Entity
{
    public  class Wallet
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public string WalletType { get; set; } 
        public virtual ICollection<WalletTransaction> Transactions { get; set; }
    }
    public class WalletTransaction
    {
        public int TransactionId { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // Deposit, Payment, Refund
        public DateTime CreatedDate { get; set; }
        public int ?OrderId { get; set; } // OrderId hoặc PaymentId
    }
}
