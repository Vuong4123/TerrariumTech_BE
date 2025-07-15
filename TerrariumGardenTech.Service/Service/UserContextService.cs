using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TerrariumGardenTech.Repositories.Enums;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    public int GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");
        }

        var identity = user.Identity;
        if (identity == null || !identity.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
        }

        return int.Parse(userIdClaim);
    }

    public RoleStatus GetCurrentUserRole()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
        }

        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

        if (!Enum.TryParse<RoleStatus>(roleClaim, out var role))
        {
            throw new UnauthorizedAccessException("Không xác định được vai trò người dùng.");
        }

        return role;
    }


}