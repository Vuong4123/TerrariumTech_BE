using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class TransportRepository : GenericRepository<OrderTransport>
    {
        public TransportRepository(TerrariumGardenTechDBContext dBContext) : base(dBContext)
        {}
    }
}
