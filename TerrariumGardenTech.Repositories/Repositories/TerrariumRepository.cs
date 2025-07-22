using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class TerrariumRepository : GenericRepository<Terrarium>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public TerrariumRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Terrarium>> FilterTerrariumsAsync(int? environmentId, int? shapeId, int? tankMethodId)
    {
        var query = _context.Terrariums.AsQueryable();

        if (environmentId.HasValue) query = query.Where(t => t.EnvironmentId == environmentId.Value);

        if (shapeId.HasValue) query = query.Where(t => t.ShapeId == shapeId.Value);

        if (tankMethodId.HasValue) query = query.Where(t => t.TankMethodId == tankMethodId.Value);

        return await query
                .Include(t => t.TerrariumImages)  // Nạp dữ liệu TerrariumImages
                .ToListAsync();
    }

    public async Task<IEnumerable<Terrarium>> GetAllByTankMethodIdAsync(int tankMethodId)
    {
        return await _context.Terrariums
            .Where(t => t.TankMethodId == tankMethodId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Terrarium>> GetAllByShapeIdAsync(int shapeId)
    {
        return await _context.Terrariums
            .Where(t => t.ShapeId == shapeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Terrarium>> GetAllByEnvironmentIdAsync(int environmentId)
    {
        return await _context.Terrariums
            .Where(t => t.EnvironmentId == environmentId)
            .ToListAsync();
    }
    // Nạp dữ liệu Terrarium cùng với ảnh (TerrariumImages)
    public async Task<(IEnumerable<Terrarium>, int)> GetFilterAndPagedAsync(TerrariumGetAllRequest request)
    {
        var queryable = _dbContext.Terrariums.AsQueryable(); 
        queryable = Include(queryable, request.IncludeProperties);
        var totalOrigin = queryable.Count();

        // filter
        
        // end
        
        queryable = request.Pagination.IsPagingEnabled ? GetQueryablePagination(queryable, request) : queryable;

        return (await queryable.ToListAsync(), totalOrigin);
    }
    // Lấy dữ liệu Terrarium theo ID kèm theo hình ảnh
    public async Task<Terrarium> GetTerrariumWithImagesByIdAsync(int id)
    {
        // Sử dụng Include để nạp dữ liệu TerrariumImages liên quan
        return await _dbContext.Terrariums
            .Include(t => t.TerrariumImages) // Nạp dữ liệu TerrariumImages
            .Include(t => t.TerrariumAccessory)  // Nạp dữ liệu Accessory liên quan
                .ThenInclude(ta => ta.Accessory)  // Nạp dữ liệu Accessory
            .FirstOrDefaultAsync(t => t.TerrariumId == id); // Tìm theo ID// Lọc theo TerrariumId
    }

    // Lấy danh sách Terrarium theo danh sách ID để delete theo accessory
    public async Task<List<Terrarium>> GetTerrariumByIdsAsync(List<int> terrariumIds)
    {
        return await _dbContext
            .Terrariums
            .Where(t => terrariumIds.Contains(t.TerrariumId)).ToListAsync();
    }
}