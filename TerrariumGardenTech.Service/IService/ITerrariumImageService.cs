using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.TerrariumImage;

namespace TerrariumGardenTech.Service.IService;

public interface ITerrariumImageService
{
    Task<IBusinessResult> CreateTerrariumImageAsync(TerrariumImageCreateRequest terrariumImageCreateRequest);
    Task<IBusinessResult> UpdateTerrariumImageAsync(TerrariumImageUpdateRequest terrariumImageUpdateRequest);
    Task<IBusinessResult> DeleteTerrariumImageAsync(int Id);
    Task<IBusinessResult?> GetTerrariumImageByIdAsync(int Id);
    Task<IBusinessResult> GetByTerrariumId(int terrariumId);
    Task<IBusinessResult> GetAllTerrariumImageAsync();
}