using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class NotificationRepository : GenericRepository<Notification>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    //public NotificationRepository() { }

    public NotificationRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
        _context = dbContext;
    }
}