using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.TankMethod;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class TankMethodService : ITankMethodService
{
    private readonly UnitOfWork _unitOfWork;

    public TankMethodService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetAllTankMethodAsync()
    {
        var tankMethod = await _unitOfWork.TankMethod.GetAllAsync();
        if (tankMethod == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, tankMethod);
    }

    public async Task<IBusinessResult> GetTankMethodByIdAsync(int Id)
    {
        var tankMethod = await _unitOfWork.TankMethod.GetByIdAsync(Id);
        if (tankMethod == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, tankMethod);
    }

    public async Task<IBusinessResult> UpdateTankMethodAsync(TankMethodUpdateRequest tankMethodUpdateRequest)
    {
        try
        {
            var result = -1;
            var tankMethod = await _unitOfWork.TankMethod.GetByIdAsync(tankMethodUpdateRequest.TankMethodId);
            if (tankMethod != null)
            {
                _unitOfWork.TankMethod.Context().Entry(tankMethod).CurrentValues.SetValues(tankMethodUpdateRequest);
                result = await _unitOfWork.TankMethod.UpdateAsync(tankMethod);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, tankMethod);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> CreateTankMethodAsync(TankMethodCreateRequest tankMethodCreateRequest)
    {
        try
        {
            var tankMethod = new TankMethod
            {
                TankMethodType = tankMethodCreateRequest.TankMethodType,
                TankMethodDescription = tankMethodCreateRequest.TankMethodDescription
            };
            var result = await _unitOfWork.TankMethod.CreateAsync(tankMethod);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, tankMethod);
            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteTankMethodAsync(int Id)
    {
        var tankMethod = await _unitOfWork.TankMethod.GetByIdAsync(Id);
        if (tankMethod == null)
            return new BusinessResult(Const.FAIL_READ_CODE, "Tank method không tồn tại.");

        // Lấy các terrarium (kèm include cần thiết trong repo, nếu có)
        var terrariums = await _unitOfWork.Terrarium.GetAllByTankMethodIdAsync(Id);

        // Dùng transaction cho cả 2 nhánh, để an toàn
        await using var tx = await _unitOfWork.TankMethod.BeginTransactionAsync();
        try
        {
            if (terrariums != null && terrariums.Any())
            {
                foreach (var t in terrariums)
                {
                    // Xoá các thực thể liên quan nếu có (tuỳ schema dự án)
                    if (t.TerrariumImages is not null && t.TerrariumImages.Count > 0)
                        await _unitOfWork.TerrariumImage.RemoveRangeAsync(t.TerrariumImages.ToList());

                    if (t.TerrariumVariants is not null && t.TerrariumVariants.Count > 0)
                        await _unitOfWork.TerrariumVariant.RemoveRangeAsync(t.TerrariumVariants.ToList());

                    // Nếu có bảng nối TerrariumAccessory, thêm đoạn xoá dưới đây:
                    // var ta = _unitOfWork.TerrariumAccessory.Context()
                    //          .Where(x => x.TerrariumId == t.TerrariumId).ToList();
                    // if (ta.Count > 0) _unitOfWork.TerrariumAccessory.Context().RemoveRange(ta);

                    await _unitOfWork.Terrarium.RemoveAsync(t);
                }
            }

            // Xoá TankMethod (kể cả khi không có terrarium liên quan)
            var removed = await _unitOfWork.TankMethod.RemoveAsync(tankMethod);
            if (!removed)
            {
                await tx.RollbackAsync();
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Xoá tank method thất bại.");
            }

            await _unitOfWork.SaveAsync();
            await tx.CommitAsync();

            var msg = (terrariums != null && terrariums.Any())
                ? "Đã xoá tank method và toàn bộ terrarium liên quan."
                : "Đã xoá tank method (không có terrarium liên quan).";

            return new BusinessResult(Const.SUCCESS_DELETE_CODE, msg);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    //private async Task DeleteRelatedTerrariumAsync(Terrarium terrarium)
    //{
    //    // Xóa các đối tượng liên quan đến Terrarium
    //    var terrariumTankMethods = await _unitOfWork.TerrariumTankMethod.GetTankMethodsByTerrariumId(terrarium.TerrariumId);
    //    foreach (var terrariumTankMethod in terrariumTankMethods)
    //    {
    //        await _unitOfWork.TerrariumTankMethod.RemoveAsync(terrariumTankMethod);
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
}