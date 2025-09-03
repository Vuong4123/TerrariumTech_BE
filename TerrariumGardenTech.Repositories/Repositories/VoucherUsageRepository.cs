using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class VoucherUsageRepository : GenericRepository<VoucherUsage>
{
    private readonly TerrariumGardenTechDBContext _db;

    public VoucherUsageRepository(TerrariumGardenTechDBContext db) : base(db)
    {
        _db = db;
    }
}
