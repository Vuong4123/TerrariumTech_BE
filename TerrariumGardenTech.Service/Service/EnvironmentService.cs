using Sprache;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Environment;

namespace TerrariumGardenTech.Service.Service
{
    public class EnvironmentService(UnitOfWork _unitOfWork) : IEnvironmentService
    {
        


        public async Task<IBusinessResult> GetAllEnvironmentsAsync()
        {
            var environment = await _unitOfWork.Environment.GetAllAsync();
            if (environment == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, environment);
        }

        public async Task<IBusinessResult?> GetEnvironmentByIdAsync(int environmentId)
        {
            var environment = await _unitOfWork.Environment.GetByIdAsync(environmentId);
            if (environment == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
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
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, envir);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                    }
                }
                else
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
                }
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
                    EnvironmentDescription = environmentCreateRequest.EnvironmentDescription,
                };
                var resdult = await _unitOfWork.Environment.CreateAsync(envir);
                if (resdult != 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG);
                }
                return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);

            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> DeleteEnvironmentAsync(int environmentId)
        {
            // Kiểm tra sự tồn tại của Environment
            var environment = await _unitOfWork.Environment.GetByIdAsync(environmentId);
            if (environment == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG); 
            }

            // Lấy tất cả Terrarium liên quan đến TankMethod
            var terrariums = await _unitOfWork.Terrarium.GetAllByEnvironmentIdAsync(environmentId);
            if (terrariums != null && terrariums.Any())
            {
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
                        var result = await _unitOfWork.Environment.RemoveAsync(environment);
                        if (result)
                        {
                            await transaction.CommitAsync();
                            return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Tank method and related terrariums deleted successfully.");
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete tank method.");
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
                    }
                }
            }

            return new BusinessResult(Const.FAIL_READ_CODE, "No related terrariums found.");
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
}
