using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Order;
using TerrariumGardenTech.Service.ResponseModel.Order;

namespace TerrariumGardenTech.Service.IService;

public interface IOrderService
{
    Task<IEnumerable<OrderResponse>> GetAllAsync();
    Task<OrderResponse?> GetByIdAsync(int id);
    Task<int> CreateAsync(OrderCreateRequest request);
    Task<bool> UpdateStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
    Task<IBusinessResult> CheckoutAsync(int orderId, string paymentMethod, decimal paidAmount);
}