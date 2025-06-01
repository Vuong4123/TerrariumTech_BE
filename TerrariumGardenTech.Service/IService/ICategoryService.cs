using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.RequestModel.Category;

namespace TerrariumGardenTech.Service.IService
{
    public interface ICategoryService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateCategory(CategoryRequest categoryRequest);
        Task<IBusinessResult> UpdateCategory(CategoryRequest categoryRequest);
        Task<IBusinessResult> Save(Category category);
        Task<IBusinessResult> DeleteById(int id);
    }
}
