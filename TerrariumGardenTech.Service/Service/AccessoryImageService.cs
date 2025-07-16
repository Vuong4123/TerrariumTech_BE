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
using TerrariumGardenTech.Service.RequestModel.AccessoryImage;
using TerrariumGardenTech.Service.RequestModel.TerrariumImage;

namespace TerrariumGardenTech.Service.Service
{
    public class AccessoryImageService(UnitOfWork _unitOfWork) : IAccessoryImageService
    {
        

        public async Task<IBusinessResult> GetAll()
        {
            var accessoryImages = _unitOfWork.AccessoryImage.GetAllAsync();
            if (accessoryImages != null )
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryImages);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> GetById(int id)
        {
            var accessoryImage = await _unitOfWork.AccessoryImage.GetByIdAsync(id);
            if (accessoryImage != null)
            {
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryImage);
            }
            else
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
            }
        }

        public async Task<IBusinessResult> UpdateAccessory(AccessoryImageUpdateRequest accessoryImageUpdateRequest)
        {
            try
            {
                var result = -1;
                var accessoryImage = await _unitOfWork.AccessoryImage.GetByIdAsync(accessoryImageUpdateRequest.AccessoryImageId);
                if (accessoryImage != null)
                {
                    _unitOfWork.AccessoryImage.Context().Entry(accessoryImage).CurrentValues.SetValues(accessoryImageUpdateRequest);
                    result = await _unitOfWork.AccessoryImage.UpdateAsync(accessoryImage);
                    if (result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, accessoryImage);
                    }
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
                }
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG, "Terrarium image not found.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }
        public async Task<IBusinessResult> CreateAccessory(AccessoryImageCreateRequest accessoryImageCreateRequest)
        {
            try
            {
                var accessoryImage = new AccessoryImage
                {
                    AccessoryId = accessoryImageCreateRequest.AccessoryId,
                    ImageUrl = accessoryImageCreateRequest.ImageUrl,
                    AltText = accessoryImageCreateRequest.AltText,
                    IsPrimary = accessoryImageCreateRequest.IsPrimary ?? false
                };
                var result = await _unitOfWork.AccessoryImage.CreateAsync(accessoryImage);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, accessoryImage);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return new BusinessResult(Const.UNAUTHORIZED_CODE, ex.Message);
            }
            
        }

        public async Task<IBusinessResult> DeleteById(int id)
        {
            var accessoryImage = await _unitOfWork.AccessoryImage.GetByIdAsync(id);
            if (accessoryImage != null)
            {
                var result = await _unitOfWork.AccessoryImage.RemoveAsync(accessoryImage);
                if (result)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
                }
                else
                {
                    return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
                }
            }            
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
    }
}
