using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class ComboRepository : GenericRepository<Combo>
{
    public ComboRepository(TerrariumGardenTechDBContext context) : base(context)
    {
    }

    public async Task<List<Combo>> GetCombosByCategoryAsync(int categoryId)
    {
        return await _context.Combo
            .Include(c => c.ComboCategory)
            .Include(c => c.ComboItems)
            .Where(c => c.ComboCategoryId == categoryId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<Combo>> GetFeaturedCombosAsync(int take = 10)
    {
        return await _context.Combo
            .Include(c => c.ComboCategory)
            .Include(c => c.ComboItems)
            .Where(c => c.IsFeatured && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Combo>> GetCombosWithFiltersAsync(GetCombosRequest request)
    {
        var query = BuildFilterQuery(request);

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Apply pagination
        return await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();
    }

    public async Task<int> CountCombosWithFiltersAsync(GetCombosRequest request)
    {
        var query = BuildFilterQuery(request);
        return await query.CountAsync();
    }

    public async Task<List<Combo>> GetActiveCombosAsync()
    {
        return await _context.Combo
            .Include(c => c.ComboCategory)
            .Include(c => c.ComboItems)
            .Where(c => c.IsActive)
            .ToListAsync();
    }

    public async Task<List<Combo>> GetCombosByIdsAsync(List<int> comboIds)
    {
        return await _context.Combo
            .Include(c => c.ComboCategory)
            .Include(c => c.ComboItems)
            .Where(c => comboIds.Contains(c.ComboId))
            .ToListAsync();
    }

    public async Task<bool> HasActiveOrdersAsync(int comboId)
    {
        return await _context.Orders
            .AnyAsync(o => o.OrderItems.Any(oi => oi.ComboId == comboId &&
                (o.Status == OrderStatusEnum.Pending || o.Status == OrderStatusEnum.Processing)));
    }

    public async Task<Combo?> GetByIdAsync(int id)
    {
        return await _context.Combo
            .Include(c => c.ComboCategory)
            .Include(c => c.ComboItems)
            .FirstOrDefaultAsync(c => c.ComboId == id);
    }

    public async Task<List<Combo>> GetAllAsync()
    {
        return await _context.Combo
            .Include(c => c.ComboCategory)
            .Include(c => c.ComboItems)
            .ToListAsync();
    }

    #region Private Helper Methods

    private IQueryable<Combo> BuildFilterQuery(GetCombosRequest request)
    {
        var query = _context.Combo
            .Include(c => c.ComboCategory)
            .Include(c => c.ComboItems)
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(c => c.ComboCategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(searchTerm) ||
                                   (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(c => c.ComboPrice >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(c => c.ComboPrice <= request.MaxPrice.Value);
        }

        if (request.IsFeatured.HasValue)
        {
            query = query.Where(c => c.IsFeatured == request.IsFeatured.Value);
        }

        if (!request.IncludeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return query;
    }

    private IQueryable<Combo> ApplySorting(IQueryable<Combo> query, string sortBy, string sortOrder)
    {
        return sortBy.ToLower() switch
        {
            "price" => sortOrder.ToLower() == "desc" ?
                query.OrderByDescending(c => c.ComboPrice) :
                query.OrderBy(c => c.ComboPrice),
            "created" => sortOrder.ToLower() == "desc" ?
                query.OrderByDescending(c => c.CreatedAt) :
                query.OrderBy(c => c.CreatedAt),
            "sold" => sortOrder.ToLower() == "desc" ?
                query.OrderByDescending(c => c.SoldQuantity) :
                query.OrderBy(c => c.SoldQuantity),
            "discount" => sortOrder.ToLower() == "desc" ?
                query.OrderByDescending(c => c.DiscountPercent) :
                query.OrderBy(c => c.DiscountPercent),
            _ => sortOrder.ToLower() == "desc" ?
                query.OrderByDescending(c => c.Name) :
                query.OrderBy(c => c.Name),
        };
    }

    #endregion Private Helper Methods
}

public class ComboCategoryRepository : GenericRepository<ComboCategory>
{
    public ComboCategoryRepository(TerrariumGardenTechDBContext context) : base(context)
    {
    }

    public async Task<List<ComboCategory>> GetActiveCategoriesAsync()
    {
        return await _context.ComboCategory
            .Include(cc => cc.Combos)
            .Where(cc => cc.IsActive)
            .OrderBy(cc => cc.DisplayOrder)
            .ThenBy(cc => cc.Name)
            .ToListAsync();
    }

    public async Task<ComboCategory?> GetCategoryWithCombosAsync(int categoryId)
    {
        return await _context.ComboCategory
            .Include(cc => cc.Combos)
            .FirstOrDefaultAsync(cc => cc.ComboCategoryId == categoryId);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = _context.ComboCategory.Where(cc => cc.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(cc => cc.ComboCategoryId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<ComboCategory>> GetAllAsync()
    {
        return await _context.ComboCategory
            .Include(cc => cc.Combos)
            .OrderBy(cc => cc.DisplayOrder)
            .ThenBy(cc => cc.Name)
            .ToListAsync();
    }

    public async Task<ComboCategory?> GetByIdAsync(int id)
    {
        return await _context.ComboCategory
            .Include(cc => cc.Combos)
            .FirstOrDefaultAsync(cc => cc.ComboCategoryId == id);
    }
}

public class ComboItemRepository : GenericRepository<ComboItem>
{
    public ComboItemRepository(TerrariumGardenTechDBContext context) : base(context)
    {
    }

    public async Task<List<ComboItem>> GetItemsByComboIdAsync(int comboId)
    {
        return await _context.ComboItem
            .Where(ci => ci.ComboId == comboId)
            .ToListAsync();
    }

    public async Task<List<ComboItem>> GetItemsByComboIdsAsync(List<int> comboIds)
    {
        return await _context.ComboItem
            .Where(ci => comboIds.Contains(ci.ComboId))
            .ToListAsync();
    }

    public async Task<int> DeleteByComboIdAsync(int comboId)
    {
        var items = await _context.ComboItem
            .Where(ci => ci.ComboId == comboId)
            .ToListAsync();

        _context.ComboItem.RemoveRange(items);
        return items.Count;
    }
}