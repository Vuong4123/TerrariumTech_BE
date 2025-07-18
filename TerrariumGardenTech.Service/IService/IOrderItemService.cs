using TerrariumGardenTech.Service.RequestModel.OrderItem;
using TerrariumGardenTech.Service.ResponseModel.OrderItem;

namespace TerrariumGardenTech.Service.IService;

public interface IOrderItemService
{
    Task<IEnumerable<OrderItemSummaryResponse>> GetAllAsync();
    Task<OrderItemSummaryResponse?> GetByIdAsync(int id);
    Task<IEnumerable<OrderItemSummaryResponse>> GetByOrderAsync(int orderId);
    Task<int> CreateAsync(CreateOrderItemRequest request);
    Task<bool> UpdateAsync(int id, CreateOrderItemRequest request);
    Task<bool> DeleteAsync(int id);
}