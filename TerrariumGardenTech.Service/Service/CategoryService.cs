using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Category;
using TerrariumGardenTech.Common.ResponseModel.Category;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class CategoryService : ICategoryService
{
    private readonly UnitOfWork _unitOfWork;

    public CategoryService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }
    // Implement methods for ICategoryService here
    // For example, you can add methods like GetAllCategories, GetCategoryById, etc.


    public async Task<IBusinessResult> GetAll()
    {
        // Lấy tất cả danh mục từ repository
        var categories = await _unitOfWork.Category.GetAllAsync();

        // Kiểm tra kết quả
        if (categories == null || !categories.Any())
        {
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");
        }

        // Ánh xạ từ entity sang DTO (CategoryUpdateRequest)
        var categoryResponses = categories.Select(c => new CategoryResponse
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            Description = c.Description
        }).ToList();

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", categoryResponses);
    }

    public async Task<IBusinessResult> GetById(int id)
    {
        // Lấy Category theo ID từ repository
        var category = await _unitOfWork.Category.GetByIdAsync(id);

        // Kiểm tra nếu Category không tồn tại
        if (category == null)
        {
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Category not found.");
        }

        // Ánh xạ từ entity sang DTO (CategoryResponse)
        var categoryResponse = new CategoryResponse
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", categoryResponse);
    }

    public async Task<IBusinessResult> Save(Category category)
    {
        try
        {
            var result = -1;
            var categoryyEntity = _unitOfWork.Category.GetByIdAsync(category.CategoryId);
            if (categoryyEntity != null)
            {
                result = await _unitOfWork.Category.UpdateAsync(category);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, category);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            // Create new terrarium if it does not exist
            result = await _unitOfWork.Category.CreateAsync(category);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, category);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> UpdateCategory(CategoryUpdateRequest categoryRequest)
    {
        try
        {
            var result = -1;
            var cate = await _unitOfWork.Category.GetByIdAsync(categoryRequest.CategoryId);
            if (cate != null)
            {
                _unitOfWork.Category.Context().Entry(cate).CurrentValues.SetValues(categoryRequest);
                result = await _unitOfWork.Category.UpdateAsync(cate);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, cate);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> CreateCategory(CategoryCreateRequest categoryRequest)
    {
        var category = new Category
        {
            CategoryName = categoryRequest.CategoryName,
            Description = categoryRequest.Description
        };
        var result = await _unitOfWork.Category.CreateAsync(category);
        if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, category);

        return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
    }

    public Task<IBusinessResult> DeleteById(int id)
    {
        var category = _unitOfWork.Category.GetByIdAsync(id);
        if (category != null)
        {
            var result = _unitOfWork.Category.RemoveAsync(category.Result);
            if (result.Result)
                return Task.FromResult<IBusinessResult>(new BusinessResult(Const.SUCCESS_DELETE_CODE,
                    Const.SUCCESS_DELETE_MSG));

            return Task.FromResult<IBusinessResult>(new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG));
        }

        return Task.FromResult<IBusinessResult>(new BusinessResult(Const.WARNING_NO_DATA_CODE,
            Const.WARNING_NO_DATA_MSG));
    }
}