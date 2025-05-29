using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.RequestModel.TerrariumCategory;

namespace TerrariumGardenTech.Service.IService
{
    public interface ITerrariumCategoryService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateTerrariumCategory(TerrariumCategoryRequest terrariumCategoryRequest);
        Task<IBusinessResult> UpdateTerrariumCategory(TerrariumCategoryRequest terrariumCategoryRequest);
        Task<IBusinessResult> Save(TerrariumCategory terrariumCategory);
        Task<IBusinessResult> DeleteById(int id);
    }
}
