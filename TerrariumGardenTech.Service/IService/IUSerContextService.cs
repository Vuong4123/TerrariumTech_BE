using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.Service.IService
{
    public interface IUserContextService
    {

        public int GetCurrentUser();
        RoleStatus GetCurrentUserRole();
    }
}
