using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.RequestModel.Pagination;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IOrderService
{
    Task<IBusinessResult> GetAllAsync();
    Task<IBusinessResult> GetByIdAsync(int orderId);
    Task<int> CreateAsync(OrderCreateRequest request);

    Task<bool> UpdateStatusAsync(int id, OrderStatusEnum status);
    Task<bool> DeleteAsync(int id);
    Task<IBusinessResult> CheckoutAsync(int orderId, string paymentMethod);
    Task<IEnumerable<OrderResponse>> GetByUserAsync(int userId);


    Task<(bool, string, object?)> RequestRefundAsync(CreateRefundRequest request, int currentUserId);
    Task<(bool, string, object?)> UpdateRequestRefundAsync(UpdateRefundRequest request, int currentUserId);

    Task<IBusinessResult> GetAllWithPaginationAsync(PaginationRequest request);
    Task<IBusinessResult> GetByUserWithPaginationAsync(int userId, PaginationRequest request);
}