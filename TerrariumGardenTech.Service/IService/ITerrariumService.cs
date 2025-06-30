using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Terrarium;

namespace TerrariumGardenTech.Service.IService
{
    public interface ITerrariumService
    {
        Task<IBusinessResult> GetAll();
        // Task<BusinessResult> GetAllOfParam(string type, string shape, string tankMethod, string theme, string size);
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateTerrarium(TerrariumCreateRequest terrariumCreateRequest);
        Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest);
        Task<IBusinessResult> Save(Terrarium terrarium);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> GetTerrariumByNameAsync(string terrariumName);
    }
}
