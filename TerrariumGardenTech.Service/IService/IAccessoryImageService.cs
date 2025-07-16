using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.RequestModel.AccessoryImage;

namespace TerrariumGardenTech.Service.IService
{
    public interface IAccessoryImageService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateAccessory(AccessoryImageCreateRequest accessoryImageCreateRequest);
        Task<IBusinessResult> UpdateAccessory(AccessoryImageUpdateRequest accessoryImageUpdateRequest);
        Task<IBusinessResult> DeleteById(int id);
    }
}
