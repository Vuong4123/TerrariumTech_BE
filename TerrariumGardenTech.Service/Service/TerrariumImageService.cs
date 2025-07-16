using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Environment;
using TerrariumGardenTech.Service.RequestModel.Terrarium;
using TerrariumGardenTech.Service.RequestModel.TerrariumImage;

namespace TerrariumGardenTech.Service.Service
{
    public class TerrariumImageService : ITerrariumImageService
    {
        private readonly UnitOfWork _unitOfWork;
        public TerrariumImageService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> GetAllTerrariumImageAsync()
        {
            var terraImage = await _unitOfWork.TerrariumImage.GetAllAsync();
            if (terraImage == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terraImage);
        }

        public async Task<IBusinessResult?> GetTerrariumImageByIdAsync(int Id)
        {
            var terraImage = await _unitOfWork.TerrariumImage.GetByIdAsync(Id);
            if (terraImage == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
            }
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terraImage);
        }

        public async Task<IBusinessResult> UpdateTerrariumImageAsync(TerrariumImageUpdateRequest terrariumImageUpdateRequest)
        {
            try
            {
                var result = -1;
                var terraImage = await _unitOfWork.TerrariumImage.GetByIdAsync(terrariumImageUpdateRequest.TerrariumImageId);
                if(terraImage != null)
                {
                    _unitOfWork.TerrariumImage.Context().Entry(terraImage).CurrentValues.SetValues(terrariumImageUpdateRequest);
                    result = await _unitOfWork.TerrariumImage.UpdateAsync(terraImage);
                    if(result > 0)
                    {
                        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, terraImage);
                    }
                    return new BusinessResult(Const.FAIL_UPDATE_CODE,Const.FAIL_UPDATE_MSG);
                }
                return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG, "Terrarium image not found.");
            }
            catch (Exception ex) 
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }
        

        public async Task<IBusinessResult> DeleteTerrariumImageAsync(int environmentId)
        {
            var result = await _unitOfWork.TerrariumImage.GetByIdAsync(environmentId);
            if(result != null)
            {
                var deleteResult = await _unitOfWork.TerrariumImage.RemoveAsync(result);
                if(deleteResult)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
                }
                return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
            }
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG, "Terrarium image not found.");
        }

        public async Task<IBusinessResult> CreateTerrariumImageAsync(TerrariumImageCreateRequest terrariumImageCreateRequest)
        {
            try
            {
                var terraImage = new TerrariumImage
                {
                    TerrariumId = terrariumImageCreateRequest.TerrariumId,
                    ImageUrl = terrariumImageCreateRequest.ImageUrl,
                    AltText = terrariumImageCreateRequest.AltText,
                    IsPrimary = terrariumImageCreateRequest.IsPrimary ?? false

                };
                var result = await _unitOfWork.TerrariumImage.CreateAsync(terraImage);
                if (result > 0)
                {
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, terraImage);
                }
                return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }
    }
}
