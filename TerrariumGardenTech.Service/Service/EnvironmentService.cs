using Azure.Core;
using Sprache;
using System.Reflection.Metadata;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Blog;
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
            var envir = await _unitOfWork.Environment.GetByIdAsync(environmentId);
            if (envir != null)
            {
                var result = await _unitOfWork.Environment.RemoveAsync(envir);
                if (result)
                {
                    return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
                }
                return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);

            }
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        }
    }
}
