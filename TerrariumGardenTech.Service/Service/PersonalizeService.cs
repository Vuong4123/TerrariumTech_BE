﻿using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Personalize;

namespace TerrariumGardenTech.Service.Service;

public class PersonalizeService(UnitOfWork _unitOfWork, IUserContextService userContextService) : IPersonalizeService
{
    public async Task<IBusinessResult> GetAllPersonalize()
    {
        var result = await _unitOfWork.Personalize.GetAllAsync();
        if (result == null) return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, result);
    }

    public async Task<IBusinessResult> GetPersonalizeById(int id)
    {
        var result = await _unitOfWork.Personalize.GetByIdAsync(id);
        if (result == null) return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, result);
    }

    public async Task<IBusinessResult> SavePersonalize(Personalize personalize)
    {
        try
        {
            var result = -1;
            var personalizeEntity = _unitOfWork.Personalize.GetByIdAsync(personalize.PersonalizeId);
            if (personalizeEntity != null)
            {
                result = await _unitOfWork.Personalize.UpdateAsync(personalize);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, personalize);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            // Create new terrarium if it does not exist
            result = await _unitOfWork.Personalize.CreateAsync(personalize);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, personalize);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> CreatePersonalize(PersonalizeCreateRequest personalizeCreateRequest)
    {
        try
        {
            var GetCurrentUser = userContextService.GetCurrentUser();
            var personalize = new Personalize
            {
                UserId = GetCurrentUser,
                Type = personalizeCreateRequest.Type,
                Shape = personalizeCreateRequest.Shape,
                TankMethod = personalizeCreateRequest.TankMethod,
                Theme = personalizeCreateRequest.Theme,
                Size = personalizeCreateRequest.Size
            };
            var result = await _unitOfWork.Personalize.CreateAsync(personalize);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, result);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> UpdatePersonalize(PersonalizeUpdateRequest personalizeUpdateRequest)
    {
        try
        {
            var result = -1;
            var personalize = await _unitOfWork.Personalize.GetByIdAsync(personalizeUpdateRequest.PersonalizeId);
            if (personalize != null)
            {
                _unitOfWork.Personalize.Context().Entry(personalize).CurrentValues.SetValues(personalizeUpdateRequest);
                result = await _unitOfWork.Personalize.UpdateAsync(personalize);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, personalize);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> DeletePersonalizeById(int id)
    {
        var personalize = await _unitOfWork.Personalize.GetByIdAsync(id);
        if (personalize != null)
        {
            var result = await _unitOfWork.Personalize.RemoveAsync(personalize);
            if (result) return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);

            return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
        }

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }
}