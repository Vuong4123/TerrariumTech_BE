using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class PaymentTransitionRepository : GenericRepository<PaymentTransition>
{
    public PaymentTransitionRepository(TerrariumGardenTechDBContext context) : base(context)
    {
    }

    public async Task AddAsync(PaymentTransition entity)
    {
        await _context.Set<PaymentTransition>().AddAsync(entity);
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}