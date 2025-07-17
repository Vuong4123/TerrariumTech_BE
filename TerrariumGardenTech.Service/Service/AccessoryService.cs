using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Accessory;

namespace TerrariumGardenTech.Service.Service
{
    public class AccessoryService : IAccessoryService
    {
        private readonly UnitOfWork _unitOfWork;
        public AccessoryService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }



        public async Task<IBusinessResult> GetAll()
        {
            var accessoryList = await _unitOfWork.Accessory.GetAllAsync();
            if (accessoryList != null && accessoryList.Any())
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryList);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> GetById(int id)
        {
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(id);
            if (accessory != null)
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessory);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> Save(Accessory accessory)
        {
            try
            {
                int result = -1;
                var accessoryEntity = _unitOfWork.Accessory.GetByIdAsync(accessory.AccessoryId);
                if (accessoryEntity != null)
                {
                    result = await _unitOfWork.Accessory.UpdateAsync(accessory);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, accessory);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                    }
                }
                else
                {
                    // Create new terrarium if it does not exist
                    result = await _unitOfWork.Accessory.CreateAsync(accessory);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, accessory);
                    }
                    else
                    {
                        return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
                    }
                }


            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
            }
        }

        public async Task<IBusinessResult> CreateAccessory(AccessoryCreateRequest accessoryCreateRequest)
        {
            var categoryExists = await _unitOfWork.Category.AnyAsync(c => c.CategoryId == accessoryCreateRequest.CategoryId);

            if (!categoryExists)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "CategoryId không tồn tại.");
            }
            var accessory = new Accessory
            {
                Name = accessoryCreateRequest.Name,
                Size = accessoryCreateRequest.Size,
                Description = accessoryCreateRequest.Description,
                Price = accessoryCreateRequest.Price,
                StockQuantity = accessoryCreateRequest.Stock,
                CategoryId = accessoryCreateRequest.CategoryId,
                CreatedAt = accessoryCreateRequest.CreatedAt ?? DateTime.Now,
                UpdatedAt = accessoryCreateRequest.UpdatedAt ?? DateTime.Now,
                Status = accessoryCreateRequest.Status
            };
            var result = await _unitOfWork.Accessory.CreateAsync(accessory);
            if (result > 0)
            {
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, accessory);
            }
            else
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
            }

        }
        public async Task<IBusinessResult> UpdateAccessory(AccessoryUpdateRequest accessoryUpdateRequest)
        {
            try
            {
                var categoryExists = await _unitOfWork.Category.AnyAsync(c => c.CategoryId == accessoryUpdateRequest.CategoryId);

                if (!categoryExists)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "CategoryId không tồn tại.");
                }
                int result = -1;
                var access = await _unitOfWork.Accessory.GetByIdAsync(accessoryUpdateRequest.AccessoryId);
                if (access != null)
                {
                    _unitOfWork.Accessory.Context().Entry(access).CurrentValues.SetValues(accessoryUpdateRequest);
                    result = await _unitOfWork.Accessory.UpdateAsync(access);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, access);
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
        public async Task<IBusinessResult> DeleteById(int id)
        {
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(id);
            if (accessory == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
            // Xóa các bản ghi liên quan trong bảng AccessoryImage
            var relatedImages = await _unitOfWork.AccessoryImage.GetAllByAccessoryIdAsync(id);
            foreach (var image in relatedImages)
            {
                await _unitOfWork.AccessoryImage.RemoveAsync(image);  // Xóa từng hình ảnh liên quan đến Accessory
            }
            var terrariumAccessory = await _unitOfWork.TerrariumAccessory.GetAllTerrariumByAccessory(id);
            var terrariumIds = terrariumAccessory.Select(ts => ts.TerrariumId).Distinct().ToList();
            var terrariums = await _unitOfWork.Terrarium.GetTerrariumByIdsAsync(terrariumIds);

            using (var transaction = await _unitOfWork.Accessory.BeginTransactionAsync())
            {
                try
                {
                    foreach (var terrariumAccessories in terrariumAccessory)
                    {
                        await _unitOfWork.TerrariumAccessory.RemoveAsync(terrariumAccessories);
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
                    var result = await _unitOfWork.Accessory.RemoveAsync(accessory);
                    if (result)
                    {
                        //neu xoa thanh cong, commit giao dich
                        await transaction.CommitAsync();
                        return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_DELETE_MSG);
                    }
                    // xxoa that bai, huy giao dich
                    await transaction.RollbackAsync();
                    return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete the shape.");

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
            
            var terrariumImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(terrarium.TerrariumId);
            foreach (var terrariumImage in terrariumImages)
            {
                await _unitOfWork.TerrariumImage.RemoveAsync(terrariumImage);
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
