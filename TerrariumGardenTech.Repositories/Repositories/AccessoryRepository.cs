using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class AccessoryRepository : GenericRepository<Accessory>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    //public AccessoryRepository() { }
    public AccessoryRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }
    // Add any specific methods for AccessoryRepository here

    public async Task<List<Accessory?>> GetByName(List<string?> name)
    {
        return await _dbContext.Set<Accessory>()
            .Where(e => name.Contains(e.Name)) // Tìm tất cả các Accessory có tên nằm trong danh sách names
            .ToListAsync();
    }

    public async Task<IEnumerable<Accessory>> FilterAccessoryAsync(int? categoryId)
    {
        return await _context.Accessories.Where(a => a.CategoryId == categoryId).ToListAsync();
    }

    // Nạp dữ liệu Accessory cùng với ảnh (AccessoryImages)
    public async Task<IEnumerable<Accessory>> GetAllWithImagesAsync()
    {
        return await _dbContext.Accessories
            .Include(t => t.AccessoryImages) // Nạp dữ liệu AccessoryImages
            .ToListAsync();
    }
    // Lấy dữ liệu Terrarium theo ID kèm theo hình ảnh
    public async Task<Accessory> GetAccessoryWithImagesByIdAsync(int id)
    {
        // Sử dụng Include để nạp dữ liệu TerrariumImages liên quan
        return await _dbContext.Accessories
            .Include(t => t.AccessoryImages) // Nạp dữ liệu TerrariumImages
            .FirstOrDefaultAsync(t => t.AccessoryId == id); // Tìm theo ID// Lọc theo TerrariumId
    }
}