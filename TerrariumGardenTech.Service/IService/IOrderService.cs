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

    Task<bool> UpdateStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
    Task<IBusinessResult> CheckoutAsync(int orderId, string paymentMethod);
    Task<IEnumerable<OrderResponse>> GetByUserAsync(int userId);


    Task<(bool, string)> RequestRefundAsync(CreateRefundRequest request, int currentUserId);
    Task<IBusinessResult> GetRefundDetailAsync(int refundId);
    Task<IBusinessResult> GetRefundAsync(int orderId);
    Task<(bool, string, object?)> UpdateRequestRefundAsync(UpdateRefundRequest request, int currentUserId);

    Task<IBusinessResult> GetAllWithPaginationAsync(PaginationRequest request);
    Task<IBusinessResult> GetByUserWithPaginationAsync(int userId, PaginationRequest request);
    Task<IBusinessResult> AcceptRefundRequestAsync(int refundId, int staffId, AcceptRefundRequest request);
    Task<IBusinessResult> CancelOrderAsync(int orderId, int userId, CancelOrderRequest request);
    Task<IBusinessResult> GetPendingRefundRequestsAsync();
}