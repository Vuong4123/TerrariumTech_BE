using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface ITerrariumImageService
{
    Task<IBusinessResult> CreateTerrariumImageAsync(IFormFile imageFile, int terrariumId);
    Task<IBusinessResult> UpdateTerrariumImageAsync(int terrariumImageId, IFormFile? newImageFile);
    Task<IBusinessResult> DeleteTerrariumImageAsync(int Id);
    Task<IBusinessResult?> GetTerrariumImageByIdAsync(int Id);
    Task<IBusinessResult> GetByTerrariumId(int terrariumId);
    Task<IBusinessResult> GetAllTerrariumImageAsync();
}