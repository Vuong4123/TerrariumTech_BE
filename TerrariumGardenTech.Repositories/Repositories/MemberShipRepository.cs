using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;
public class MemberShipRepository : GenericRepository<Membership>
{
 private readonly TerrariumGardenTechDBContext _dbContext;

    public MemberShipRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;
}