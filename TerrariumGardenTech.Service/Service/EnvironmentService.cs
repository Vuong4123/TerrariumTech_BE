using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Environment;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class EnvironmentService(UnitOfWork _unitOfWork) : IEnvironmentService
{
    public async Task<IBusinessResult> GetAllEnvironmentsAsync()
    {
        var environment = await _unitOfWork.Environment.GetAllAsync();
        if (environment == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, environment);
    }

    public async Task<IBusinessResult?> GetEnvironmentByIdAsync(int environmentId)
    {
        var environment = await _unitOfWork.Environment.GetByIdAsync(environmentId);
        if (environment == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, environment);
    }

    public async Task<IBusinessResult> UpdateEnvironmentAsync(EnvironmentUpdateRequest environmentUpdateRequest)
    {
        try
        {
            var result = -1;
            var envir = await _unitOfWork.Environment.GetByIdAsync(environmentUpdateRequest.EnvironmentId);
            if (envir != null)
            {
                _unitOfWork.Environment.Context().Entry(envir).CurrentValues.SetValues(environmentUpdateRequest);
                result = await _unitOfWork.Environment.UpdateAsync(envir);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, envir);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }


    public async Task<IBusinessResult> CreateEnvironmentAsync(EnvironmentCreateRequest environmentCreateRequest)
    {
        try
        {
            var envir = new EnvironmentTerrarium
            {
                EnvironmentName = environmentCreateRequest.EnvironmentName,
                EnvironmentDescription = environmentCreateRequest.EnvironmentDescription
            };
            var resdult = await _unitOfWork.Environment.CreateAsync(envir);
            if (resdult != 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG);
            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteEnvironmentAsync(int environmentId)
    {
        // 1) Kiểm tra Environment tồn tại
        var environment = await _unitOfWork.Environment.GetByIdAsync(environmentId);
        if (environment == null)
            return new BusinessResult(Const.FAIL_READ_CODE, "Environment không tồn tại.");

        // 2) Lấy terrarium theo EnvironmentId
        var terrariums = await _unitOfWork.Terrarium.GetAllByEnvironmentIdAsync(environmentId);

        await using var tx = await _unitOfWork.Environment.BeginTransactionAsync();
        try
        {
            // 3) Nếu có terrarium liên quan, xoá các thực thể phụ trước rồi xoá terrarium
            if (terrariums != null && terrariums.Any())
            {
                foreach (var t in terrariums)
                {
                    if (t.TerrariumImages is not null && t.TerrariumImages.Count > 0)
                        await _unitOfWork.TerrariumImage.RemoveRangeAsync(t.TerrariumImages.ToList());

                    if (t.TerrariumVariants is not null && t.TerrariumVariants.Count > 0)
                        await _unitOfWork.TerrariumVariant.RemoveRangeAsync(t.TerrariumVariants.ToList());

                    // Nếu có bảng nối TerrariumAccessory thì xoá luôn (nếu dự án bạn dùng):
                    // var tas = _unitOfWork.TerrariumAccessory.Context()
                    //     .Where(x => x.TerrariumId == t.TerrariumId).ToList();
                    // if (tas.Count > 0) _unitOfWork.TerrariumAccessory.Context().RemoveRange(tas);

                    await _unitOfWork.Terrarium.RemoveAsync(t);
                }
            }

            // 4) Xoá chính Environment
            var removed = await _unitOfWork.Environment.RemoveAsync(environment);
            if (!removed)
            {
                await tx.RollbackAsync();
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Xoá environment thất bại.");
            }

            await _unitOfWork.SaveAsync();
            await tx.CommitAsync();

            var msg = (terrariums != null && terrariums.Any())
                ? "Đã xoá environment và toàn bộ terrarium liên quan."
                : "Đã xoá environment (không có terrarium liên quan).";

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
    //    var terrariumImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(terrarium.TerrariumId);
    //    foreach (var terrariumImage in terrariumImages)
    //    {
    //        await _unitOfWork.TerrariumImage.RemoveAsync(terrariumImage);
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