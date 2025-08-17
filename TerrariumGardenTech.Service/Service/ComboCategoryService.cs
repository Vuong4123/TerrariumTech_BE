using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Common.ResponseModel.Combo;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

public class ComboCategoryService : IComboCategoryService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ILogger<ComboCategoryService> _logger;

    public ComboCategoryService(UnitOfWork unitOfWork, ILogger<ComboCategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IBusinessResult> GetAllCategoriesAsync(bool includeInactive = false)
    {
        try
        {
            List<ComboCategory> categories;

            if (includeInactive)
            {
                categories = await _unitOfWork.ComboCategory.GetAllAsync();
            }
            else
            {
                categories = await _unitOfWork.ComboCategory.GetActiveCategoriesAsync();
            }

            var result = categories.Select(c => new ComboCategoryResponse
            {
                ComboCategoryId = c.ComboCategoryId,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder,
                TotalCombos = c.Combos.Count,
                ActiveCombos = c.Combos.Count(combo => combo.IsActive),
                CreatedAt = c.CreatedAt
            }).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách danh mục combo thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combo categories");
            return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy danh mục combo: {ex.Message}");
        }
    }

    public async Task<IBusinessResult> CreateCategoryAsync(CreateComboCategoryRequest request)
    {
        try
        {
            // Kiểm tra trùng tên
            var nameExists = await _unitOfWork.ComboCategory.ExistsByNameAsync(request.Name);
            if (nameExists)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Tên danh mục đã tồn tại");
            }

            var category = new ComboCategory
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var result = await _unitOfWork.ComboCategory.CreateAsync(category);
            if (result <= 0)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Tạo danh mục combo thất bại");
            }

            await _unitOfWork.SaveAsync();

            var response = new ComboCategoryResponse
            {
                ComboCategoryId = category.ComboCategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                TotalCombos = 0,
                ActiveCombos = 0,
                CreatedAt = category.CreatedAt
            };

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Tạo danh mục combo thành công", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating combo category");
            return new BusinessResult(Const.FAIL_CREATE_CODE, $"Lỗi khi tạo danh mục combo: {ex.Message}");
        }
    }

    public async Task<IBusinessResult> UpdateCategoryAsync(UpdateComboCategoryRequest request)
    {
        try
        {
            var category = await _unitOfWork.ComboCategory.GetByIdAsync(request.ComboCategoryId);
            if (category == null)
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy danh mục combo");
            }

            // Kiểm tra trùng tên (trừ chính nó)
            var nameExists = await _unitOfWork.ComboCategory.ExistsByNameAsync(request.Name, request.ComboCategoryId);
            if (nameExists)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Tên danh mục đã tồn tại");
            }

            // Update properties
            category.Name = request.Name.Trim();
            category.Description = request.Description?.Trim();
            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;
            category.UpdatedAt = DateTime.Now;

            await _unitOfWork.ComboCategory.UpdateAsync(category);
            await _unitOfWork.SaveAsync();

            var response = new ComboCategoryResponse
            {
                ComboCategoryId = category.ComboCategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                TotalCombos = category.Combos.Count,
                ActiveCombos = category.Combos.Count(c => c.IsActive),
                CreatedAt = category.CreatedAt
            };

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật danh mục combo thành công", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating combo category");
            return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi cập nhật danh mục combo: {ex.Message}");
        }
    }

    public async Task<IBusinessResult> GetCategoryByIdAsync(int id)
    {
        try
        {
            var category = await _unitOfWork.ComboCategory.GetByIdAsync(id);
            if (category == null)
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy danh mục combo");
            }

            var result = new ComboCategoryResponse
            {
                ComboCategoryId = category.ComboCategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                TotalCombos = category.Combos.Count,
                ActiveCombos = category.Combos.Count(c => c.IsActive),
                CreatedAt = category.CreatedAt
            };

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy thông tin danh mục combo thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting combo category by id: {Id}", id);
            return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy danh mục combo: {ex.Message}");
        }
    }

    public async Task<IBusinessResult> DeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _unitOfWork.ComboCategory.GetCategoryWithCombosAsync(id);
            if (category == null)
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy danh mục combo");
            }

            // Kiểm tra có combo nào đang hoạt động không
            if (category.Combos.Any(c => c.IsActive))
            {
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Không thể xóa danh mục đang có combo hoạt động");
            }

            await _unitOfWork.ComboCategory.RemoveAsync(category);
            await _unitOfWork.SaveAsync();

            return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Xóa danh mục combo thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting combo category");
            return new BusinessResult(Const.FAIL_DELETE_CODE, $"Lỗi khi xóa danh mục combo: {ex.Message}");
        }
    }
}
