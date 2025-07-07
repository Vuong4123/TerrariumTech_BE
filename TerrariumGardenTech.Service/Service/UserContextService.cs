using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class UserContextService(IHttpContextAccessor httpContextAccessor) : IUserContextService
{
    public int GetCurrentUser()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return int.Parse(userIdClaim);
    }
}