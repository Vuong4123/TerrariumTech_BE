using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.TerrariumImage;

namespace TerrariumGardenTech.Service.IService;

public interface ITerrariumImageService
{
    Task<IBusinessResult> CreateTerrariumImageAsync(IFormFile imageFile, int terrariumId);
    Task<IBusinessResult> UpdateTerrariumImageAsync(TerrariumImageUploadUpdateRequest request);
    Task<IBusinessResult> DeleteTerrariumImageAsync(int Id);
    Task<IBusinessResult?> GetTerrariumImageByIdAsync(int Id);
    Task<IBusinessResult> GetByTerrariumId(int terrariumId);
    Task<IBusinessResult> GetAllTerrariumImageAsync();
}