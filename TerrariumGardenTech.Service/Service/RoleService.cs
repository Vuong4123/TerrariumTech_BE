using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Role;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class RoleService : IRoleService
{
    private readonly UnitOfWork _unitOfWork;

    public RoleService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IBusinessResult> GetAll()
    {
        var roles = await _unitOfWork.Role.GetAllAsync();
        if (roles != null && roles.Any())
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, roles);

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }

    public async Task<IBusinessResult> GetById(int id)
    {
        var role = await _unitOfWork.Role.GetByIdAsync(id);
        if (role != null) return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, role);

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }

    public async Task<IBusinessResult> Save(Role role)
    {
        try
        {
            var result = -1;
            var roleEntity = await _unitOfWork.Role.GetByIdAsync(role.RoleId);
            if (roleEntity != null)
            {
                result = await _unitOfWork.Role.UpdateAsync(role);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, role);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            result = await _unitOfWork.Role.CreateAsync(role);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, role);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateRole(RoleUpdateRequest roleUpdateRequest)
    {
        try
        {
            var result = -1;
            var role = await _unitOfWork.Role.GetByIdAsync(roleUpdateRequest.RoleId);
            if (role != null)
            {
                _unitOfWork.Role.Context().Entry(role).CurrentValues.SetValues(roleUpdateRequest);
                result = await _unitOfWork.Role.UpdateAsync(role);
                if (result > 0) return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, role);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> CreateRole(RoleCreateRequest roleCreateRequest)
    {
        try
        {
            var role = new Role
            {
                RoleName = roleCreateRequest.RoleName,
                Description = roleCreateRequest.Description
            };
            var result = await _unitOfWork.Role.CreateAsync(role);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, role);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> DeleteById(int id)
    {
        var role = _unitOfWork.Role.GetById(id);
        if (role != null)
        {
            var result = await _unitOfWork.Role.RemoveAsync(role);
            if (result) return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);

            return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
        }

        return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
    }
}