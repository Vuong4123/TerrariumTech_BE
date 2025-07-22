using TerrariumGardenTech.Common.RequestModel.BlogCategory;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IBlogCategoryService
{
    Task<IBusinessResult> GetAllBlogCategory();
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> CreateBlogCategory(BlogCategoryCreateRequest blogCategoryRequest);
    Task<IBusinessResult> UpdateBlogCategory(BlogCategoryUpdateRequest blogCategoryRequest);
    Task<IBusinessResult> Save(BlogCategory blogCategory);
    Task<IBusinessResult> DeleteById(int id);
}