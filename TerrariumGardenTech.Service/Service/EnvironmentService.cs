using Sprache;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Environment;

namespace TerrariumGardenTech.Service.Service
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly UnitOfWork _unitOfWork;
        public EnvironmentService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


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

            var terrariumEnvironments = await _unitOfWork.TerrariumEnvironment.GetAllTerrariumByEnvironment(environmentId);
            var terrariumIds = terrariumEnvironments.Select(te => te.TerrariumId).Distinct().ToList();
            var terrariums = await _unitOfWork.Terrarium.GetTerrariumByIdsAsync(terrariumIds);

            using (var transaction = await _unitOfWork.Environment.BeginTransactionAsync())
            {
                try
                {
                    foreach (var terrariumEnvironment in terrariumEnvironments)
                    {
                        await _unitOfWork.TerrariumEnvironment.RemoveAsync(terrariumEnvironment);
                    }

                    // Xóa các Terrarium và các đối tượng liên quan
                    if (terrariums != null)
                    {
                        foreach (var terrarium in terrariums)
                        {
                            // Xóa các đối tượng liên quan đến Terrarium
                            await DeleteRelatedTerrariumAsync(terrarium);
                        }
                    }

                    // Xóa Environment
                    var result = await _unitOfWork.Environment.RemoveAsync(environment);
                    if (result)
                    {
                        // Nếu xóa thành công, commit giao dịch
                        await transaction.CommitAsync();
                        return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
                    }

                    // Nếu xóa thất bại, hủy giao dịch
                    await transaction.RollbackAsync();
                    return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
                }
                catch (Exception )
                {
                    // Nếu có lỗi, hủy giao dịch và ghi log
                    await transaction.RollbackAsync();
                    return new BusinessResult(Const.FAIL_DELETE_CODE, "An error occurred while deleting the environment.");
                }
            }
        }

        private async Task DeleteRelatedTerrariumAsync(Terrarium terrarium)
        {
            // Xóa các đối tượng liên quan đến Terrarium
            var terrariumTankMethods = await _unitOfWork.TerrariumTankMethod.GetTankMethodsByTerrariumId(terrarium.TerrariumId);
            foreach (var terrariumTankMethod in terrariumTankMethods)
            {
                await _unitOfWork.TerrariumTankMethod.RemoveAsync(terrariumTankMethod);
            }

            var terrariumImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(terrarium.TerrariumId);
            foreach (var terrariumImage in terrariumImages)
            {
                await _unitOfWork.TerrariumImage.RemoveAsync(terrariumImage);
            }

            var terrariumShapes = await _unitOfWork.TerrariumShape.GetTerrariumShapesByTerrariumIdAsync(terrarium.TerrariumId);
            foreach (var terrariumShape in terrariumShapes)
            {
                await _unitOfWork.TerrariumShape.RemoveAsync(terrariumShape);
            }

            var terrariumAccessories = await _unitOfWork.TerrariumAccessory.GetTerrariumAccessoriesByTerrariumAsync(terrarium.TerrariumId);
            foreach (var terrariumAccessory in terrariumAccessories)
            {
                await _unitOfWork.TerrariumAccessory.RemoveAsync(terrariumAccessory);
            }

            var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllByTerrariumIdAsync(terrarium.TerrariumId);
            foreach (var terrariumVariant in terrariumVariants)
            {
                await _unitOfWork.TerrariumVariant.RemoveAsync(terrariumVariant);
            }

            await _unitOfWork.Terrarium.RemoveAsync(terrarium);
        }

    }
}
