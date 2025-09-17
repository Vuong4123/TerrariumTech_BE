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
    public class PaymentConfigRepository : GenericRepository<PaymentConfig>
    {
        private readonly TerrariumGardenTechDBContext _dbContext;

        public PaymentConfigRepository(TerrariumGardenTechDBContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
