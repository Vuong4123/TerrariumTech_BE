using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface ICloudinaryService
{
    Task<IBusinessResult> UploadImageAsync(IFormFile file, string folder, string publicId = null);
    Task<IBusinessResult> DeleteImageAsync(string imageUrl);

    
}