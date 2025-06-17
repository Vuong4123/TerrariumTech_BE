using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Blog;
using TerrariumGardenTech.Service.RequestModel.BlogCategory;

namespace TerrariumGardenTech.Service.IService
{
    public interface IBlogCategoryService
    {
        Task<IBusinessResult> GetAllBlogCategory();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateBlogCategory(BlogCategoryCreateRequest blogCategoryRequest);
        Task<IBusinessResult> UpdateBlogCategory(BlogCategoryUpdateRequest blogCategoryRequest);
        Task<IBusinessResult> Save(BlogCategory blogCategory);
        Task<IBusinessResult> DeleteById(int id);
    }
}
