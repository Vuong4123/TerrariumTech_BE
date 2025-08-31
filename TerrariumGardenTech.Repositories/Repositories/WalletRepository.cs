using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;

        public WalletRepository(TerrariumGardenTechDBContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Wallet?> GetByUserIdAndTypeAsync(int? userId, string walletType)
        {
            return await _dbContext.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId && w.WalletType == walletType);
        }
        // WalletRepository
        public async Task<Wallet> GetByUserIdAsync(int userId)
        {
            return await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        }

        // WalletTransactionRepository
        public async Task<List<WalletTransaction>> GetTransactionsByWalletIdAndDateRangeAsync(
            int walletId, DateTime fromDate, DateTime toDate)
        {
            return await _dbContext.WalletTransaction
                .Where(t => t.WalletId == walletId &&
                           t.CreatedDate >= fromDate &&
                           t.CreatedDate <= toDate)
                .OrderBy(t => t.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<WalletTransaction>> GetAllTransactionsByWalletIdAsync(int walletId)
        {
            return await _dbContext.WalletTransaction
                .Where(t => t.WalletId == walletId)
                .OrderBy(t => t.CreatedDate)
                .ToListAsync();
        }

    }
}
