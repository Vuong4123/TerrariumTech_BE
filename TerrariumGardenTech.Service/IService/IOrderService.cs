
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Order;

using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IOrderService
{
    Task<IEnumerable<OrderResponse>> GetAllAsync();
    Task<OrderResponse?> GetByIdAsync(int id);
    Task<int> CreateAsync(OrderCreateRequest request);

    Task<bool> UpdateStatusAsync(int id, OrderStatus status);
    Task<bool> DeleteAsync(int id);
    Task<IBusinessResult> CheckoutAsync(int orderId, string paymentMethod);
    Task<IEnumerable<OrderResponse>> GetByUserAsync(int userId);

}