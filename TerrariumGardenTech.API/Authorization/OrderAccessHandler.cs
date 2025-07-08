using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TerrariumGardenTech.Repositories.Repositories;

namespace TerrariumGardenTech.API.Authorization
{
    public class OrderAccessHandler
    : AuthorizationHandler<OrderAccessRequirement, int>   // int = orderId
    {
        private readonly OrderRepository _repo;
        public OrderAccessHandler(OrderRepository repo) => _repo = repo;

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OrderAccessRequirement requirement,
            int orderId)
        {
            // 1) Vai trò đặc quyền
            if (context.User.IsInRole("Admin") ||
                context.User.IsInRole("Manager") ||
                context.User.IsInRole("Staff") ||
                context.User.IsInRole("Shipper"))
            {
                context.Succeed(requirement);
                return;
            }

            // 2) Kiểm tra chủ sở hữu
            int userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var order = await _repo.GetByIdAsync(orderId);

            if (order is not null && order.UserId == userId)
                context.Succeed(requirement);
        }
    }
}
