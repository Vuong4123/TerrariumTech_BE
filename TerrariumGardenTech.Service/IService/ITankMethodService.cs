using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Shape;
using TerrariumGardenTech.Service.RequestModel.TankMethod;

public interface ITankMethodService
{
    Task<IBusinessResult> CreateShapeAsync(TankMethodCreateRequest tankMethodCreateRequest);
    Task<IBusinessResult> UpdateShapeAsync(TankMethodUpdateRequest tankMethodUpdateRequest);
    Task<IBusinessResult> DeleteShapeAsync(int Id);
    Task<IBusinessResult> GetShapeByIdAsync(int Id);
    Task<IBusinessResult> GetAllShapesAsync();
}