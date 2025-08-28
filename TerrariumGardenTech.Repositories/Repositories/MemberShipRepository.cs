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

    public async Task<List<Membership>> GetAllWithDetailsAsync()
    {
        return await _context.Memberships
            .Include(m => m.Package)
            .Include(m => m.User)
            .ToListAsync();
    }

    public async Task<List<Membership>> GetMembershipsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Memberships
            .Include(m => m.Package)
            .Include(m => m.User)
            .Where(m => m.StartDate >= startDate && m.StartDate <= endDate)
            .ToListAsync();
    }

    public async Task<List<Membership>> GetActiveMembershipsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Memberships
            .Include(m => m.Package)
            .Include(m => m.User)
            .Where(m => m.Status == "Active" && m.EndDate >= now)
            .ToListAsync();
    }

    public async Task<List<Membership>> GetMembershipsByStatusAsync(string status)
    {
        return await _context.Memberships
            .Include(m => m.Package)
            .Include(m => m.User)
            .Where(m => m.Status == status)
            .ToListAsync();
    }

    public async Task<List<Membership>> GetMembershipsByUserIdAsync(int userId)
    {
        return await _context.Memberships
            .Include(m => m.Package)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.StartDate)
            .ToListAsync();
    }

    public async Task<Membership> GetLatestMembershipByUserIdAsync(int userId)
    {
        return await _context.Memberships
            .Include(m => m.Package)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.StartDate)
            .FirstOrDefaultAsync();
    }
}