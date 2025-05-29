using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.RequestModel.Terrarium;

namespace TerrariumGardenTech.Service.Service
{
    public class AccessoryService : IAccessoryService
    {
        private readonly UnitOfWork _unitOfWork;
        public AccessoryService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(id);
            if (accessory != null)
            {
                var result = await _unitOfWork.Accessory.RemoveAsync(accessory);
                if (result)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
                }
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
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
                Description = accessoryCreateRequest.Description,
                Price = accessoryCreateRequest.Price,
                Stock = accessoryCreateRequest.Stock,
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
    }
}
