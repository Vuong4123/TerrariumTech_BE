using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Shape;

public interface IShapeService
{
    Task<IBusinessResult> CreateShapeAsync(ShapeCreateRequest shapeCreateRequest);
    Task<IBusinessResult> UpdateShapeAsync(ShapeUpdateRequest shapeUpdateRequest);
    Task<IBusinessResult> DeleteShapeAsync(int shapeId);
    Task<IBusinessResult> GetShapeByIdAsync(int shapeId);
    Task<IBusinessResult> GetAllShapesAsync();
}