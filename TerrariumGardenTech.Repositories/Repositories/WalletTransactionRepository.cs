using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>
    {
        public WalletTransactionRepository(TerrariumGardenTechDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<WalletTransaction>> GetTransactionsByStatus(string Status)
        {
            return await _context.WalletTransaction
                .Where(t => t.Type == Status)
                .ToListAsync();
        }
    }
}