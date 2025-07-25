using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class AddressRepository : GenericRepository<Address>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public AddressRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task<IEnumerable<Address>> GetByUserIdAsync(int  userId)
    {
        return await _dbContext.Addresses
            .Where(a => a.UserId == userId) // Bạn có thể thay thế "Contains" bằng cách tìm chính xác tên nếu cần
            .ToListAsync();
    }
}