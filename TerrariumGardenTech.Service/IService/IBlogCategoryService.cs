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
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateBlogCaTegory(BlogCategoryRequest blogCategoryRequest);
        Task<IBusinessResult> UpdateBlogCategory(BlogCategoryRequest blogCategoryRequest);
        Task<IBusinessResult> Save(BlogCategory blogCategory);
        Task<IBusinessResult> DeleteById(int id);
    }
}
