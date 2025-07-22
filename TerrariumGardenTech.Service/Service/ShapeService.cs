using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Shape;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class ShapeService : IShapeService
{
    private readonly UnitOfWork _unitOfWork;

    public ShapeService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<IBusinessResult> GetAllShapesAsync()
    {
        var shapes = await _unitOfWork.Shape.GetAllAsync();
        if (shapes == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, shapes);
    }

    public async Task<IBusinessResult> GetShapeByIdAsync(int shapeId)
    {
        var shape = await _unitOfWork.Shape.GetByIdAsync(shapeId);
        if (shape == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, shape);
    }

    public async Task<IBusinessResult> CreateShapeAsync(ShapeCreateRequest shapeCreateRequest)
    {
        try
        {
            var shape = new Shape
            {
                ShapeName = shapeCreateRequest.ShapeName,
                ShapeDescription = shapeCreateRequest.ShapeDescription,
                ShapeMaterial = shapeCreateRequest.ShapeMaterial
            };

            var result = await _unitOfWork.Shape.CreateAsync(shape);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, result);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateShapeAsync(ShapeUpdateRequest shapeUpdateRequest)
    {
        try
        {
            var result = -1;
            var shape = await _unitOfWork.Shape.GetByIdAsync(shapeUpdateRequest.ShapeId);
            if (shape != null)
            {
                _unitOfWork.Shape.Context().Entry(shape).CurrentValues.SetValues(shapeUpdateRequest);
                result = await _unitOfWork.Shape.UpdateAsync(shape);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, result);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteShapeAsync(int shapeId)
    {
        //Kiem tra ton tai cua Shape
        var shape = await _unitOfWork.Shape.GetByIdAsync(shapeId);
        if (shape == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        // Lấy tất cả Terrarium liên quan đến TankMethod
        var terrariums = await _unitOfWork.Terrarium.GetAllByEnvironmentIdAsync(shapeId);
        if (terrariums != null && terrariums.Any())
            using (var transaction = await _unitOfWork.Environment.BeginTransactionAsync())
            {
                try
                {
                    // Xóa các Terrarium liên quan
                    foreach (var terrarium in terrariums)
                    {
                        // Xóa tất cả các bản ghi liên quan đến Terrarium (TerrariumImage, TerrariumVariant, ...)
                        await _unitOfWork.TerrariumImage.RemoveRangeAsync(terrarium.TerrariumImages);
                        await _unitOfWork.TerrariumVariant.RemoveRangeAsync(terrarium.TerrariumVariants.ToList());

                        // Xóa Terrarium
                        await _unitOfWork.Terrarium.RemoveAsync(terrarium);
                    }

                    // Sau khi xóa tất cả Terrarium, xóa Environment
                    var result = await _unitOfWork.Shape.RemoveAsync(shape);
                    if (result)
                    {
                        await transaction.CommitAsync();
                        return new BusinessResult(Const.SUCCESS_DELETE_CODE,
                            "Tank method and related terrariums deleted successfully.");
                    }

                    await transaction.RollbackAsync();
                    return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete tank method.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
                }
            }

        return new BusinessResult(Const.FAIL_READ_CODE, "No related terrariums found.");
    }

    // else
    // {
    //     var result = await _unitOfWork.Shape.RemoveAsync(shape);
    //     if (result)
    //     {
    //         return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG, result);
    //     }
    //     else
    //     {
    //         return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
    //     }
    // }
}

//private async Task DeleteRelatedTerrariumAsync(Terrarium terrarium)
//{
//    // Xóa các đối tượng liên quan đến Terrarium
//    var terrariumShapes = await _unitOfWork.TerrariumShape.GetTerrariumShapesByTerrariumIdAsync(terrarium.TerrariumId);
//    foreach (var terrariumShape in terrariumShapes)
//    {
//        await _unitOfWork.TerrariumShape.RemoveAsync(terrariumShape);
//    }

//    var terrariumImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(terrarium.TerrariumId);
//    foreach (var terrariumImage in terrariumImages)
//    {
//        await _unitOfWork.TerrariumImage.RemoveAsync(terrariumImage);
//    }

//    var terrariumEnvironment = await _unitOfWork.TerrariumEnvironment.GetTerrariumEnvironmentByTerrariumIdAsync(terrarium.TerrariumId);
//    foreach (var terrariumEnvironmentItem in terrariumEnvironment)
//    {
//        await _unitOfWork.TerrariumShape.RemoveAsync(terrariumEnvironmentItem);
//    }

//    var terrariumAccessories = await _unitOfWork.TerrariumAccessory.GetTerrariumAccessoriesByTerrariumAsync(terrarium.TerrariumId);
//    foreach (var terrariumAccessory in terrariumAccessories)
//    {
//        await _unitOfWork.TerrariumAccessory.RemoveAsync(terrariumAccessory);
//    }

//    var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllByTerrariumIdAsync(terrarium.TerrariumId);
//    foreach (var terrariumVariant in terrariumVariants)
//    {
//        await _unitOfWork.TerrariumVariant.RemoveAsync(terrariumVariant);
//    }

//    await _unitOfWork.Terrarium.RemoveAsync(terrarium);
//}