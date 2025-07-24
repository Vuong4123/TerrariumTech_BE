using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class PaymentRepository : GenericRepository<Payment>
{
    public PaymentRepository(TerrariumGardenTechDBContext context) : base(context)
    {
    }

    public async Task AddAsync(Payment entity)
    {
        await _context.Set<Payment>().AddAsync(entity);
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}