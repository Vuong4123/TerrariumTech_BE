using DotNetEnv;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Environment;
using TerrariumGardenTech.Service.RequestModel.TankMethod;

namespace TerrariumGardenTech.Service.Service
{
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
            if (tankMethod == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, tankMethod);

        }

        public async Task<IBusinessResult> GetTankMethodByIdAsync(int Id)
        {
            var tankMethod = await _unitOfWork.TankMethod.GetByIdAsync(Id);
            if (tankMethod == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
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
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, tankMethod);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                    }
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
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, tankMethod);
                }
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
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
            var terrariumTankMethods = await _unitOfWork.TerrariumTankMethod.GetAllTerrariumByTankMethods(Id);
            var terrariumIds = terrariumTankMethods.Select(ts => ts.TerrariumId).Distinct().ToList();
            var terrariums = await _unitOfWork.Terrarium.GetTerrariumByIdsAsync(terrariumIds);

            using (var transaction = await _unitOfWork.TankMethod.BeginTransactionAsync())
            {
                try
                {
                    foreach (var terrariumTankMethod in terrariumTankMethods)
                    {
                        await _unitOfWork.TerrariumTankMethod.RemoveAsync(terrariumTankMethod);
                    }
                    //Xoa cac terrarium va cac doi tuong lien quan
                    if (terrariums != null)
                    {
                        foreach (var terrarium in terrariums)
                        {
                            //xoa cac doi tuong lien quan den Terrarium
                            await DeleteRelatedTerrariumAsync(terrarium);
                        }
                    }
                    var result = await _unitOfWork.TankMethod.RemoveAsync(tankMethod);
                    if (result)
                    {
                        //neu xoa thanh cong, commit giao dich
                        await transaction.CommitAsync();
                        return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
                    }
                    // xoa that bai, huy giao dich
                    await transaction.RollbackAsync();
                    return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);

                }
                catch (Exception)
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

            var terrariumEnvironment = await _unitOfWork.TerrariumEnvironment.GetTerrariumEnvironmentByTerrariumIdAsync(terrarium.TerrariumId);
            foreach (var terrariumEnvironmentItem in terrariumEnvironment)
            {
                await _unitOfWork.TerrariumShape.RemoveAsync(terrariumEnvironmentItem);
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
