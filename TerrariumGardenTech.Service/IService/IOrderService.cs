using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.RequestModel.Transports;
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


    Task<(bool, string)> RequestRefundAsync(CreateRefundRequest request, int currentUserId);
    Task<(bool, string)> UpdateRequestRefundAsync(UpdateRefundRequest request, int currentUserId);



}