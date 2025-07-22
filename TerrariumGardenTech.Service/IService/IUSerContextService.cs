using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Service.IService;

public interface IUserContextService
{
    public int GetCurrentUser();
    RoleStatus GetCurrentUserRole();
}