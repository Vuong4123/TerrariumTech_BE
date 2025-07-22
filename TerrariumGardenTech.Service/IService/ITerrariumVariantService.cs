using TerrariumGardenTech.Common.RequestModel.TerrariumVariant;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface ITerrariumVariantService
{
    Task<IBusinessResult> CreateTerrariumVariantAsync(TerrariumVariantCreateRequest terrariumVariantCreateRequest);
    Task<IBusinessResult> UpdateTerrariumVariantAsync(TerrariumVariantUpdateRequest terrariumVariantUpdateRequest);
    Task<IBusinessResult> DeleteTerrariumVariantAsync(int id);
    Task<IBusinessResult?> GetTerrariumVariantByIdAsync(int Id);
    Task<IBusinessResult> GetAllVariantByTerrariumIdAsync(int terrariumId);
    Task<IBusinessResult> GetAllTerrariumVariantAsync();
}