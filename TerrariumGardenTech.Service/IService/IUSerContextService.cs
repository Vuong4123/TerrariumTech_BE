using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Service.IService;

public interface IUserContextService
{
    int GetCurrentUser();
    RoleStatus GetCurrentUserRole();
    string GetCurrentUsername();           // (Tùy chọn mở rộng)
    bool IsAuthenticated();                // (Tùy chọn)
}