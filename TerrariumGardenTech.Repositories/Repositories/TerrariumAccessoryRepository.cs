using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

public class TerrariumAccessoryRepository : GenericRepository<TerrariumAccessory>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public TerrariumAccessoryRepository(TerrariumGardenTechDBContext dbContext) => _dbContext = dbContext;

    // public async Task<List<TerrariumAccessory>> GetAccessoriesByTerrariumIdAsync(int terrariumId)
    // {
    //     return await _dbContext.Set<TerrariumAccessory>()
    //         .Where(ta => ta.TerrariumId == terrariumId)
    //         .ToListAsync();
    // }

    // public async Task AddAccessoriesToTerrariumAsync(int terrariumId, List<int> accessoryIds)
    // {
    //     foreach (var accessoryId in accessoryIds)
    //     {
    //         var terrariumAccessory = new TerrariumAccessory
    //         {
    //             TerrariumId = terrariumId,
    //             AccessoryId = accessoryId
    //         };
    //         await _dbContext.Set<TerrariumAccessory>().AddAsync(terrariumAccessory);
    //     }
    //     await _dbContext.SaveChangesAsync();
    // }
    public async Task<List<TerrariumAccessory>>GetTerrariumAccessoriesByTerrariumAsync(int terrariumId){
        return await _dbContext.
        TerrariumAccessory.
        Where(ta => ta.TerrariumId == terrariumId).ToListAsync();
    }
}