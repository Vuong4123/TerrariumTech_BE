using Microsoft.AspNetCore.Http;
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
        Task<IBusinessResult> CreateAccessory(IFormFile imageFile, int accessoryId);
        Task<IBusinessResult> UpdateAccessory(int accessoryImageId, IFormFile? newImageFile);
        Task<IBusinessResult> DeleteById(int id);
        Task<IBusinessResult> GetByAccessoryId(int accessoryId);
    }
}
