using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IAccessoryImageService
{
    Task<IBusinessResult> GetAll();
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> CreateAccessory(IFormFile imageFile, int accessoryId);
    Task<IBusinessResult> UpdateAccessory(int accessoryImageId, IFormFile? newImageFile);
    Task<IBusinessResult> DeleteById(int id);
    Task<IBusinessResult> GetByAccessoryId(int accessoryId);
}