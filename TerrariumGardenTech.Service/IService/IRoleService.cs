using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Role;

namespace TerrariumGardenTech.Service.IService;

public interface IRoleService
{
    Task<IBusinessResult> GetAll();
    Task<IBusinessResult> GetById(int id);
    Task<IBusinessResult> CreateRole(RoleCreateRequest roleCreateRequest);
    Task<IBusinessResult> UpdateRole(RoleUpdateRequest roleUpdateRequest);
    Task<IBusinessResult> Save(Role role);
    Task<IBusinessResult> DeleteById(int id);
}