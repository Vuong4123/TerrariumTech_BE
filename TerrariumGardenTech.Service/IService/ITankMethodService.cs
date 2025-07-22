using TerrariumGardenTech.Common.RequestModel.TankMethod;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface ITankMethodService
{
    Task<IBusinessResult> CreateTankMethodAsync(TankMethodCreateRequest tankMethodCreateRequest);
    Task<IBusinessResult> UpdateTankMethodAsync(TankMethodUpdateRequest tankMethodUpdateRequest);
    Task<IBusinessResult> DeleteTankMethodAsync(int Id);
    Task<IBusinessResult> GetTankMethodByIdAsync(int Id);
    Task<IBusinessResult> GetAllTankMethodAsync();
}