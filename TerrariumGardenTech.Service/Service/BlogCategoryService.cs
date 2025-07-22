using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.BlogCategory;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class BlogCategoryService : IBlogCategoryService
{
    private readonly UnitOfWork _unitOfWork;

    public BlogCategoryService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IBusinessResult> GetAllBlogCategory()
    {
        var blogCategories = await _unitOfWork.BlogCategory.GetAllAsync();
        if (blogCategories != null && blogCategories.Any())
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, blogCategories);

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }

    public async Task<IBusinessResult> GetById(int id)
    {
        var blogCategory = await _unitOfWork.BlogCategory.GetByIdAsync(id);
        if (blogCategory != null)
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, blogCategory);

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }

    public async Task<IBusinessResult> Save(BlogCategory blogCategory)
    {
        try
        {
            var result = -1;
            var blogCateEntity = _unitOfWork.BlogCategory.GetByIdAsync(blogCategory.BlogCategoryId);
            if (blogCateEntity != null)
            {
                result = await _unitOfWork.BlogCategory.UpdateAsync(blogCategory);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, blogCateEntity);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            // Create new terrarium if it does not exist
            result = await _unitOfWork.BlogCategory.CreateAsync(blogCategory);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, result);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> UpdateBlogCategory(BlogCategoryUpdateRequest blogCategoryRequest)
    {
        try
        {
            var result = -1;
            var blogCate = await _unitOfWork.BlogCategory.GetByIdAsync(blogCategoryRequest.BlogCategoryId);
            if (blogCate != null)
            {
                _unitOfWork.Blog.Context().Entry(blogCate).CurrentValues.SetValues(blogCategoryRequest);
                result = await _unitOfWork.BlogCategory.UpdateAsync(blogCate);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, blogCate);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> CreateBlogCategory(BlogCategoryCreateRequest blogCategoryRequest)
    {
        try
        {
            var blogCategory = new BlogCategory
            {
                CategoryName = blogCategoryRequest.CategoryName,
                Description = blogCategoryRequest.Description
            };
            var result = await _unitOfWork.BlogCategory.CreateAsync(blogCategory);
            if (result > 0)
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, blogCategory);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }


    public async Task<IBusinessResult> DeleteById(int id)
    {
        var blogCategory = await _unitOfWork.BlogCategory.GetByIdAsync(id);
        if (blogCategory != null)
        {
            var result = _unitOfWork.BlogCategory.RemoveAsync(blogCategory);
            if (result != null) return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);

            return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
        }

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }
}