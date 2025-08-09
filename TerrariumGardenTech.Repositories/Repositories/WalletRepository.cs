using Microsoft.EntityFrameworkCore;
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
    }
}
