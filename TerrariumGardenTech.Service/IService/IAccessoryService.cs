using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.RequestModel.Terrarium;

namespace TerrariumGardenTech.Service.IService
{
    public interface  IAccessoryService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateTerrarium(AccessoryCreateRequest accessoryCreateRequest);
        Task<IBusinessResult> UpdateTerrarium(AccessoryUpdateRequest accessoryUpdateRequest);
        Task<IBusinessResult> Save(Accessory accessory);
        Task<IBusinessResult> DeleteById(int id);
    }
}
