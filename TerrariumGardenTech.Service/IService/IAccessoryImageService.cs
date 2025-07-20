using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.AccessoryImage;

namespace TerrariumGardenTech.Service.IService;

public interface IAccessoryImageService
{
    Task<IBusinessResult> GetAll();
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> CreateAccessoryImage(IFormFile imageFile, int accessoryId);
    Task<IBusinessResult> UpdateAccessoryImage(AccessoryImageUploadUpdateRequest request);
    Task<IBusinessResult> DeleteById(int id);
    Task<IBusinessResult> GetByAccessoryId(int accessoryId);
}