using TerrariumGardenTech.Common.RequestModel.Blog;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IBlogService
{
    Task<IBusinessResult> GetAll();
    Task<IBusinessResult> GetByCategory(int categoryId);
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> CreateBlog(BlogCreateRequest blogCreateRequest);
    Task<IBusinessResult> UpdateBlog(BlogUpdateRequest blogUpdateRequest);
    Task<IBusinessResult> Save(Blog blog);
    Task<IBusinessResult> DeleteById(int id);
}