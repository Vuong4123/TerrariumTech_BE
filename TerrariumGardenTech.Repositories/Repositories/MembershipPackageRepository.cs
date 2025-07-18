using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class MembershipPackageRepository : GenericRepository<MembershipPackage>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public MembershipPackageRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
}