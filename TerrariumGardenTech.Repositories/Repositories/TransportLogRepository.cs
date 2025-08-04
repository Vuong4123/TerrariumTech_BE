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
    public class TransportLogRepository : GenericRepository<TransportLog>
    {
        public TransportLogRepository(TerrariumGardenTechDBContext dBContext) : base(dBContext)
        {

        }
    }
}
