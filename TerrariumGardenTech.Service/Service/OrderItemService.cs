using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Repositories;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.OrderItem;
using TerrariumGardenTech.Service.ResponseModel.OrderItem;

namespace TerrariumGardenTech.Service.Service;

public class OrderItemService : IOrderItemService
{
    private readonly OrderRepository _orderRepo;
    private readonly OrderItemRepository _repo;

    public OrderItemService(OrderItemRepository repo, OrderRepository orderRepo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
    }

    public async Task<IEnumerable<OrderItemSummaryResponse>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(ToResponse);
    }

    public async Task<OrderItemSummaryResponse?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("OrderItemId phải là số nguyên dương.", nameof(id));

        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return null;

        return ToResponse(entity);
    }

    public async Task<IEnumerable<OrderItemSummaryResponse>> GetByOrderAsync(int orderId)
    {
        if (orderId <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(orderId));

        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Đơn hàng ({orderId}) không tồn tại.");

        var items = await _repo.FindByOrderIdAsync(orderId);
        return items.Select(ToResponse);
    }

    public async Task<int> CreateAsync(CreateOrderItemRequest r)
    {
        ValidateRequest(r);

        var order = await _orderRepo.GetByIdAsync(r.OrderId);
        if (order == null)
            throw new KeyNotFoundException($"Đơn hàng ({r.OrderId}) không tồn tại.");

        var entity = new OrderItem
        {
            OrderId = r.OrderId,
            AccessoryId = r.AccessoryId,
            TerrariumVariantId = r.TerrariumVariantId,
            Quantity = r.Quantity,
            UnitPrice = r.UnitPrice,
            TotalPrice = r.Quantity * r.UnitPrice
        };

        try
        {
            await _repo.CreateAsync(entity);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Lỗi khi lưu OrderItem", ex);
        }

        // Recalculate order total
        await RecalculateOrderTotalAsync(r.OrderId);
        return entity.OrderItemId;
    }

    public async Task<bool> UpdateAsync(int id, CreateOrderItemRequest r)
    {
        if (id <= 0)
            throw new ArgumentException("OrderItemId phải là số nguyên dương.", nameof(id));
        ValidateRequest(r);

        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return false;

        var order = await _orderRepo.GetByIdAsync(entity.OrderId);
        if (order == null)
            throw new KeyNotFoundException($"Đơn hàng ({entity.OrderId}) không tồn tại.");

        entity.AccessoryId = r.AccessoryId;
        entity.TerrariumVariantId = r.TerrariumVariantId;
        entity.Quantity = r.Quantity;
        entity.UnitPrice = r.UnitPrice;
        entity.TotalPrice = r.Quantity * r.UnitPrice;

        try
        {
            await _repo.UpdateAsync(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Xung đột dữ liệu khi cập nhật OrderItem", ex);
        }

        // Recalculate order total
        await RecalculateOrderTotalAsync(entity.OrderId);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("OrderItemId phải là số nguyên dương.", nameof(id));

        var entity = await _repo.GetByIdAsync(id);
        if (entity == null)
            return false;

        var order = await _orderRepo.GetByIdAsync(entity.OrderId);
        if (order == null)
            throw new KeyNotFoundException($"Đơn hàng ({entity.OrderId}) không tồn tại.");

        await _repo.RemoveAsync(entity);

        // Recalculate order total
        await RecalculateOrderTotalAsync(entity.OrderId);
        return true;
    }

    private static void ValidateRequest(CreateOrderItemRequest r)
    {
        if (r.Quantity <= 0)
            throw new ArgumentException("Quantity phải lớn hơn 0.", nameof(r.Quantity));
        if (r.UnitPrice < 0)
            throw new ArgumentException("UnitPrice không được âm.", nameof(r.UnitPrice));
        if (r.AccessoryId == null && r.TerrariumVariantId == null)
            throw new ArgumentException("Phải cung cấp AccessoryId hoặc TerrariumVariantId.", nameof(r));
    }

    private async Task RecalculateOrderTotalAsync(int orderId)
    {
        var items = await _repo.FindByOrderIdAsync(orderId);
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Đơn hàng ({orderId}) không tồn tại.");

        order.TotalAmount = items.Sum(i => i.TotalPrice ?? 0m);
        await _orderRepo.UpdateAsync(order);
    }

    private static OrderItemSummaryResponse ToResponse(OrderItem o)
    {
        return new OrderItemSummaryResponse
        {
            OrderItemId = o.OrderItemId,
            OrderId = o.OrderId,
            AccessoryId = o.AccessoryId,
            TerrariumVariantId = o.TerrariumVariantId,
            Quantity = o.Quantity ?? 0,
            UnitPrice = o.UnitPrice ?? 0,
            TotalPrice = o.TotalPrice ?? 0
        };
    }
}