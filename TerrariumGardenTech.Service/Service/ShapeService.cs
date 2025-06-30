using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Shape;

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
        if (shapes == null)
        {
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);

        }
        else
        {
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, shapes);
        }
    }

    public async Task<IBusinessResult> GetShapeByIdAsync(int shapeId)
    {
        var shape = await _unitOfWork.Shape.GetByIdAsync(shapeId);
        if (shape == null)
        {
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        }
        else
        {
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, shape);
        }
    }

    public async Task<IBusinessResult> CreateShapeAsync(ShapeCreateRequest shapeCreateRequest)
    {
        try
        {
            var shape = new Shape
            {
                ShapeName = shapeCreateRequest.ShapeName,
                ShapeDescription = shapeCreateRequest.ShapeDescription,
                ShapeSize = shapeCreateRequest.ShapeSize,
                ShapeHeight = shapeCreateRequest.ShapeHeight,
                ShapeWidth = shapeCreateRequest.ShapeWidth,
                ShapeLength = shapeCreateRequest.ShapeLength,
                ShapeVolume = shapeCreateRequest.ShapeVolume,
                ShapeMaterial = shapeCreateRequest.ShapeMaterial
            };

            var result = await _unitOfWork.Shape.CreateAsync(shape);
            if (result > 0)
            {
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, result);
            }
            else
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
            }


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
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, result);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                }
            }
            else
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }


        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteShapeAsync(int shapeId)
    {
        var shape = await _unitOfWork.Shape.GetByIdAsync(shapeId);
        if (shape == null)
        {
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        }
        else
        {
            var result = await _unitOfWork.Shape.RemoveAsync(shape);
            if (result)
            {
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG, result);
            }
            else
            {
                return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
            }
        }
    }
}