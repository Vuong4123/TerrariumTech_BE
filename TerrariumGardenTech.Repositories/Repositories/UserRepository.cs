using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Repositories.Repositories;

public class UserRepository : GenericRepository<User>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public UserRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<User>> GetUsersForChatAsync(int currentUserId, RoleStatus currentUserRole)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Where(u => u.UserId != currentUserId &&
                       (u.Status == "Active" || u.Status == AccountStatus.Active.ToString()));

        switch (currentUserRole)
        {
            case RoleStatus.Admin:
                // Admin can see all users (Managers, Staff, Users)
                query = query.Where(u => u.RoleId == (int)RoleStatus.Manager ||
                                        u.RoleId == (int)RoleStatus.Staff ||
                                        u.RoleId == (int)RoleStatus.User);
                break;
            case RoleStatus.Manager:
                // Manager can see Staff and Users
                query = query.Where(u => u.RoleId == (int)RoleStatus.Staff ||
                                        u.RoleId == (int)RoleStatus.User);
                break;
            case RoleStatus.Staff:
                // Staff can see Users and other Staff
                query = query.Where(u => u.RoleId == (int)RoleStatus.User ||
                                        u.RoleId == (int)RoleStatus.Staff);
                break;
            case RoleStatus.User:
                // Users can see Staff, Managers, and Admins
                query = query.Where(u => u.RoleId == (int)RoleStatus.Staff ||
                                        u.RoleId == (int)RoleStatus.Manager ||
                                        u.RoleId == (int)RoleStatus.Admin);
                break;
            default:
                // Default: no users visible
                query = query.Where(u => false);
                break;
        }

        return await query.OrderBy(u => u.FullName).ToListAsync();
    }
}