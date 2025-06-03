using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Blog;
using TerrariumGardenTech.Service.RequestModel.Category;

namespace TerrariumGardenTech.Service.IService
{
    public interface IBlogService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateBlog(BlogCreateRequest blogCreateRequest);
        Task<IBusinessResult> UpdateBlog(BlogUpdateRequest blogUpdateRequest);
        Task<IBusinessResult> Save(Blog blog);
        Task<IBusinessResult> DeleteById(int id);
    }
}
