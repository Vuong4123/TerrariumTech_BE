using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.TerrariumVariant;

namespace TerrariumGardenTech.Service.IService;

public interface ITerrariumVariantService
{
    Task<IBusinessResult> CreateTerrariumVariantAsync(TerrariumVariantCreateRequest terrariumVariantCreateRequest);
    Task<IBusinessResult> UpdateTerrariumVariantAsync(TerrariumVariantUpdateRequest terrariumVariantUpdateRequest);
    Task<IBusinessResult> DeleteTerrariumVariantAsync(int id);
    Task<IBusinessResult?> GetTerrariumVariantByIdAsync(int Id);
    Task<IBusinessResult> GetAllTerrariumVariantAsync();
}