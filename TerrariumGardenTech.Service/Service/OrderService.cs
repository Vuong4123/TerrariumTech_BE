using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Order;
using TerrariumGardenTech.Service.ResponseModel.Order;
using TerrariumGardenTech.Service.ResponseModel.OrderItem;

namespace TerrariumGardenTech.Service.Service;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly UnitOfWork _unitOfWork;

    public OrderService(UnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync()
    {
        _logger.LogInformation("Lấy danh sách tất cả đơn hàng");
        var orders = await _unitOfWork.OrderRepository.GetAllAsync(); // Gọi từ UnitOfWork
        return orders.Select(ToResponse);
    }

    public async Task<OrderResponse?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));

        var order = await _unitOfWork.OrderRepository.GetByIdAsync(id); // Gọi từ UnitOfWork
        if (order == null)
        {
            _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId}", id);
            return null;
        }

        return ToResponse(order);
    }

    public async Task<int> CreateAsync(OrderCreateRequest request)
    {
        var order = new Order
        {
            UserId = request.UserId,
            TotalAmount = request.TotalAmount
            // Map other properties
        };

        try
        {
            await _unitOfWork.OrderRepository.CreateAsync(order);
            await _unitOfWork.SaveAsync(); // Save changes
            return order.OrderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            throw new Exception("An error occurred while creating the order.");
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        if (id <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status không được để trống.", nameof(status));

        var order = await _unitOfWork.OrderRepository.GetByIdAsync(id); // Gọi từ UnitOfWork
        if (order == null)
        {
            _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId} để cập nhật", id);
            return false;
        }

        try
        {
            order.Status = status.Trim();
            await _unitOfWork.OrderRepository.UpdateAsync(order); // Gọi từ UnitOfWork
            await _unitOfWork.SaveAsync(); // Lưu các thay đổi
            _logger.LogInformation("Cập nhật trạng thái đơn hàng {OrderId} thành {Status}", id, status);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Xung đột khi cập nhật trạng thái đơn hàng {OrderId}", id);
            throw new InvalidOperationException("Xung đột dữ liệu, vui lòng thử lại.");
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));

        var order = await _unitOfWork.OrderRepository.GetByIdAsync(id); // Gọi từ UnitOfWork
        if (order == null)
        {
            _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId} để xóa", id);
            return false;
        }

        try
        {
            await _unitOfWork.OrderRepository.RemoveAsync(order); // Gọi từ UnitOfWork
            await _unitOfWork.SaveAsync(); // Lưu các thay đổi
            _logger.LogInformation("Xóa đơn hàng {OrderId}", id);
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa đơn hàng {OrderId}", id);
            throw new InvalidOperationException("Không thể xóa đơn hàng, vui lòng thử lại.");
        }
    }

    public async Task<IBusinessResult> CheckoutAsync(int orderId, string paymentMethod, decimal paidAmount)
    {
        var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
        if (order == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Order does not exist", null);

        if (order.PaymentStatus == "Paid")
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Order already paid", null);

        // Check if the paid amount is correct
        if (paidAmount < order.TotalAmount - order.Deposit)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Paid amount is insufficient", null);

        // Update payment status
        order.PaymentStatus = "Paid";

        // Log the payment transaction
        var paymentTransition = new PaymentTransition
        {
            OrderId = order.OrderId,
            PaymentMethod = paymentMethod,
            PaymentAmount = paidAmount,
            PaymentDate = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.OrderRepository.UpdateAsync(order);
            await _unitOfWork.PaymentTransitionRepository.AddAsync(paymentTransition);
            await _unitOfWork.SaveAsync();
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Payment successful", null);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "Payment failed, please try again", null);
        }
    }


    #region Helpers

    private static void ValidateCreateRequest(OrderCreateRequest r)
    {
        if (r.UserId <= 0)
            throw new ArgumentException("UserId phải là số nguyên dương.", nameof(r.UserId));

        if (r.Items == null || !r.Items.Any())
            throw new ArgumentException("Phải có ít nhất một item trong đơn hàng.", nameof(r.Items));

        foreach (var item in r.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity của item phải lớn hơn 0.", nameof(item.Quantity));
            if (item.UnitPrice < 0)
                throw new ArgumentException("UnitPrice của item không được âm.", nameof(item.UnitPrice));
            if (item.AccessoryId == null && item.TerrariumVariantId == null)
                throw new ArgumentException("Item phải có AccessoryId hoặc TerrariumVariantId.", nameof(r.Items));
        }

        var sum = r.Items.Sum(i => i.Quantity * i.UnitPrice);
        if (r.TotalAmount != sum)
            throw new ArgumentException($"TotalAmount phải bằng tổng giá trị items ({sum}).", nameof(r.TotalAmount));

        if (r.Deposit < 0 || r.Deposit > r.TotalAmount)
            throw new ArgumentException("Deposit phải >= 0 và <= TotalAmount.", nameof(r.Deposit));
    }

    private static Order MapToEntity(OrderCreateRequest r)
    {
        return new Order
        {
            UserId = r.UserId,
            VoucherId = r.VoucherId,
            TotalAmount = r.TotalAmount,
            Deposit = r.Deposit,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            PaymentStatus = "Unpaid",
            ShippingStatus = "Unprocessed",
            OrderItems = r.Items.Select(i => new OrderItem
            {
                AccessoryId = i.AccessoryId,
                TerrariumVariantId = i.TerrariumVariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.Quantity * i.UnitPrice
            }).ToList()
        };
    }

    private static OrderResponse ToResponse(Order o)
    {
        return new OrderResponse
        {
            OrderId = o.OrderId,
            UserId = o.UserId,
            TotalAmount = o.TotalAmount,
            Deposit = o.Deposit,
            OrderDate = o.OrderDate,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            ShippingStatus = o.ShippingStatus,
            Items = o.OrderItems.Select(i => new OrderItemSummaryResponse
            {
                OrderItemId = i.OrderItemId,
                AccessoryId = i.AccessoryId,
                TerrariumVariantId = i.TerrariumVariantId,
                Quantity = i.Quantity ?? 0,
                UnitPrice = i.UnitPrice ?? 0,
                TotalPrice = i.TotalPrice ?? 0
            }).ToList()
        };
    }

    #endregion
}