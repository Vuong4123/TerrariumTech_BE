using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class MemberShipRepository : GenericRepository<Membership>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public MemberShipRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool IsMembershipExpired(int userId, int packageId)
    {
        var membership = _dbContext.Memberships
            .FirstOrDefault(f => f.UserId == userId && f.PackageId == packageId);

        if (membership == null)
        {
            // Nếu không có membership thì coi như hết hạn
            return true;
        }

        // Kiểm tra theo EndDate
        return membership.EndDate < DateTime.Now;
    }


}