using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TerrariumGardenTech.Repositories.Repositories;

namespace TerrariumGardenTech.API.Authorization;

public class OrderAccessHandler
    : AuthorizationHandler<OrderAccessRequirement, int> // int = orderId
{
    private readonly OrderRepository _repo;

    public OrderAccessHandler(OrderRepository repo)
    {
        _repo = repo;
    }

    protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    OrderAccessRequirement requirement,
    int resourceId)// có thể là orderId hoặc userId
    {
        // 1) Kiểm tra các vai trò đặc quyền
        if (context.User.IsInRole("Admin") ||
            context.User.IsInRole("Manager") ||
            context.User.IsInRole("Staff") ||
            context.User.IsInRole("Shipper"))
        {
            context.Succeed(requirement);
            return;
        }
        
        // 2) Kiểm tra chủ sở hữu (chỉ áp dụng cho các vai trò không phải Shipper)
        if (!context.User.IsInRole("Shipper"))
        {
            var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var order = await _repo.GetByIdAsync(resourceId);

            if (order is not null && order.UserId == userId)
                context.Succeed(requirement);
        }
        // 3) Bổ sung: nếu resourceId chính là userId (case: /get-all-by-userid/{userId})
        var idStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (int.TryParse(idStr, out var myUserId))
        {
            if (resourceId == myUserId)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }


}