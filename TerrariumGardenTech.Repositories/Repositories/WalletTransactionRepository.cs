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
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>
    {
        public WalletTransactionRepository(TerrariumGardenTechDBContext dbContext) : base(dbContext)
        {
        }
    }
}
