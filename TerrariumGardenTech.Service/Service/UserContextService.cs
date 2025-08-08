using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null) throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");
        var identity = user.Identity;
        if (identity == null || !identity.IsAuthenticated)
            throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("UserId")?.Value
                          ?? user.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException("UserId không hợp lệ.");
        return userId;
    }

    public RoleStatus GetCurrentUserRole()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException("Người dùng chưa xác thực.");
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value
                     ?? user.FindFirst("role")?.Value;
        if (string.IsNullOrWhiteSpace(roleClaim))
            throw new UnauthorizedAccessException("Không xác định được vai trò người dùng.");
        if (!Enum.TryParse<RoleStatus>(roleClaim, true, out var role))
            throw new UnauthorizedAccessException("Role user không hợp lệ.");
        return role;
    }

    public string GetCurrentUsername()
    {
        var user = httpContextAccessor.HttpContext?.User;
        return user?.Identity?.Name ?? string.Empty;
    }

    public bool IsAuthenticated()
    {
        var user = httpContextAccessor.HttpContext?.User;
        return user?.Identity?.IsAuthenticated ?? false;
    }
}



