using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Shape;
using TerrariumGardenTech.Common.ResponseModel.Shape;
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
        // Lấy tất cả các hình dạng từ cơ sở dữ liệu
        var shapes = await _unitOfWork.Shape.GetAllAsync();

        // Kiểm tra nếu có dữ liệu
        if (shapes != null && shapes.Any())
        {
            // Ánh xạ Shape thành ShapeResponse
            var shapeResponses = shapes.Select(r => new ShapeResponse
            {
                ShapeId = r.ShapeId,
                ShapeName = r.ShapeName,
                ShapeDescription = r.ShapeDescription,
                ShapeMaterial = r.ShapeMaterial
            }).ToList();

            // Trả về kết quả với mã thành công
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", shapeResponses);
        }

        // Trả về lỗi nếu không có dữ liệu
        return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");
    }

    public async Task<IBusinessResult> GetShapeByIdAsync(int shapeId)
    {
        // Lấy hình dạng theo ID từ cơ sở dữ liệu
        var shape = await _unitOfWork.Shape.GetByIdAsync(shapeId);

        // Kiểm tra nếu có dữ liệu
        if (shape != null)
        {
            // Ánh xạ Shape thành ShapeResponse
            var shapeResponse = new ShapeResponse
            {
                ShapeId = shape.ShapeId,
                ShapeName = shape.ShapeName,
                ShapeDescription = shape.ShapeDescription,
                ShapeMaterial = shape.ShapeMaterial
            };

            // Trả về kết quả với mã thành công
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", shapeResponse);
        }

        // Trả về lỗi nếu không có dữ liệu
        return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");
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
        // 1) Kiểm tra shape tồn tại
        var shape = await _unitOfWork.Shape.GetByIdAsync(shapeId);
        if (shape == null)
            return new BusinessResult(Const.FAIL_READ_CODE, "Shape không tồn tại.");

        // 2) Lấy terrarium theo ShapeId (đúng repo)
        var terrariums = await _unitOfWork.Terrarium.GetAllByShapeIdAsync(shapeId);

        await using var tx = await _unitOfWork.Shape.BeginTransactionAsync();
        try
        {
            // 3) Nếu có terrarium liên quan, xoá hết quan hệ trước
            if (terrariums != null && terrariums.Any())
            {
                foreach (var t in terrariums)
                {
                    if (t.TerrariumImages is not null && t.TerrariumImages.Count > 0)
                        await _unitOfWork.TerrariumImage.RemoveRangeAsync(t.TerrariumImages.ToList());

                    if (t.TerrariumVariants is not null && t.TerrariumVariants.Count > 0)
                        await _unitOfWork.TerrariumVariant.RemoveRangeAsync(t.TerrariumVariants.ToList());

                    // Nếu có bảng nối TerrariumAccessory thì xoá luôn (nếu bạn dùng)
                    // var tas = _unitOfWork.TerrariumAccessory.Context().Where(x => x.TerrariumId == t.TerrariumId).ToList();
                    // if (tas.Count > 0) _unitOfWork.TerrariumAccessory.Context().RemoveRange(tas);

                    await _unitOfWork.Terrarium.RemoveAsync(t);
                }
            }

            // 4) Xoá chính Shape
            var removed = await _unitOfWork.Shape.RemoveAsync(shape);
            if (!removed)
            {
                await tx.RollbackAsync();
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Xoá shape thất bại.");
            }

            await _unitOfWork.SaveAsync();
            await tx.CommitAsync();

            var msg = (terrariums != null && terrariums.Any())
                ? "Đã xoá shape và toàn bộ terrarium liên quan."
                : "Đã xoá shape (không có terrarium liên quan).";

            return new BusinessResult(Const.SUCCESS_DELETE_CODE, msg);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
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