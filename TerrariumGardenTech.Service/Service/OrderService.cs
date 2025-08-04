using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.RequestModel.Transports;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Common.ResponseModel.OrderItem;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly UnitOfWork _unitOfWork;
    //private readonly IUserContextService _userContextService;

    public OrderService(UnitOfWork unitOfWork,IUserContextService userContextService, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        //_userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
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
        //var currentUserId = _userContextService.GetCurrentUser();
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
        OrderStatusEnum enumStatus;
        if (Enum.TryParse<OrderStatusEnum>(status, true, out enumStatus))
        {
            _logger.LogWarning("Mã trạng thái đơn hàng với ID {OderId} không chính xác", id);
            return false;
        }    

        try
        {
            order.Status = enumStatus;
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

    public async Task<IBusinessResult> CheckoutAsync(int orderId, string paymentMethod)
    {
        var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
        if (order == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Order does not exist", null);

        if (order.PaymentStatus == "Paid")
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Order already paid", null);

        // Check if the paid amount is correct
        if (order.TotalAmount < order.TotalAmount - order.Deposit)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Paid amount is insufficient", null);

        // Update payment status
        order.PaymentStatus = "Paid";

        // Log the payment transaction
        var paymentTransition = new Payment
        {
            OrderId = order.OrderId,
            PaymentMethod = paymentMethod,
            PaymentAmount = order.TotalAmount,
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


    public async Task<(bool, string)> RequestRefundAsync(CreateRefundRequest request, int currentUserId)
    {
        var order = await _unitOfWork.OrderRepository.DbSet().FirstOrDefaultAsync(x => x.OrderId == request.OrderId && x.UserId == currentUserId);
        if (order == null)
            return (false, "Không tìm thấy thông tin hóa đơn!");

        if (order.Status != OrderStatusEnum.Completed)
            return (false, "Đơn hàng này không cho phép hoàn tiền!");

        if (string.IsNullOrEmpty(request.Reason))
            return (false, "Lý do yêu cầu hoàn tiền không được để trống!");

        var refundRequest = new OrderRequestRefund
        {
            OrderId = order.OrderId,
            Reason = request.Reason,
            Status = RequestRefundStatusEnum.Waiting,
            RequestDate = DateTime.Today            
        };
        order.Status = OrderStatusEnum.RequestRefund;
        await _unitOfWork.OrderRequestRefund.CreateAsync(refundRequest);
        await _unitOfWork.SaveAsync();
        return (true, "Yêu cầu hoàn tiền đã được gửi thành công!");
    }

    public async Task<(bool, string)> UpdateRequestRefundAsync(UpdateRefundRequest request, int currentUserId)
    {
        var refund = await _unitOfWork.OrderRequestRefund.DbSet().Include(x => x.OrderId).FirstOrDefaultAsync(x => x.RequestRefundId == request.RefundId);
        if (refund == null)
            return (false, "Không tìm thấy yêu cầu hoàn tiền!");
        if (refund.Order.UserId != currentUserId && request.Status != RequestRefundStatusEnum.Cancle)
            return (false, "Bạn không có quyền thực hiện thao tác này!");

        if (request.Status == RequestRefundStatusEnum.Waiting)
            return (false, "Trạng thái cho đơn yêu cầu hoàn tiền không hợp lệ!");

        if ((request.Status == RequestRefundStatusEnum.Cancle || request.Status == RequestRefundStatusEnum.Approved || request.Status == RequestRefundStatusEnum.Rejected) && refund.Status != RequestRefundStatusEnum.Waiting)
            return (false, "Yêu cầu hoàn tiền này không thể hủy!");

        if (request.Status == RequestRefundStatusEnum.Rejected && string.IsNullOrEmpty(request.Reason))
            return (false, "Lý do từ chối yêu cầu hoàn tiền không được để trống!");
        if (request.Status == RequestRefundStatusEnum.Approved && (!request.RefundAmount.HasValue && request.RefundAmount < 0))
            return (false, "Số tiền hoàn lại hoặc số điểm hoàn lại phải lớn hơn 0!");

        var order = await _unitOfWork.OrderRepository.FindOneAsync(x => x.OrderId == refund.OrderId);
        if (order == null)
            return (false, "Không tìm thấy đơn hàng cần hoàn tiền!");
        OrderTransport? transport = null;
        if (request.Status == RequestRefundStatusEnum.Approved && request.CreateTransport)
        {
            if (!request.EstimateCompletedDate.HasValue)
                return (false, "Vui lòng chọn ngày dự kiến sẽ lấy hàng hoàn tiền!");

            var users = await _unitOfWork.User.DbSet().AsNoTracking()
                .Where(x => x.UserId == request.UserId || x.UserId == currentUserId)
                .Select(x => new { x.UserId, x.Username }).ToArrayAsync();
            var assignUser = users.FirstOrDefault(x => x.UserId == request.UserId);
            var currentUser = users.FirstOrDefault(x => x.UserId == currentUserId);
            if ((request.UserId.HasValue && assignUser == null) || currentUser == null)
                return (false, "Thông tin người sẽ đến nhận hàng không hợp lệ!");

            transport = new OrderTransport
            {
                OrderId = refund.OrderId,
                Status = TransportStatusEnum.InCustomer,
                EstimateCompletedDate = request.EstimateCompletedDate.Value,
                Note = request.Note,
                IsRefund = true,
                UserId = assignUser?.UserId,
                CreatedDate = DateTime.Now,
                CreatedBy = currentUser.Username
            };
        }
        using (var trans = await _unitOfWork.OrderRequestRefund.BeginTransactionAsync())
        {
            refund.Status = request.Status;
            refund.ReasonModified = request.Reason;
            refund.UserModified = currentUserId;
            refund.LastModifiedDate = DateTime.Now;
            refund.IsPoint = request.IsPoint;
            if (refund.Status == RequestRefundStatusEnum.Approved)
            {
                order.Status = OrderStatusEnum.Refuning;
                refund.RefundAmount = request.RefundAmount;
                // Tích điểm của khách hàng ở đây
            }
            else
            {
                order.Status = OrderStatusEnum.Completed;
            }
            var isRollback = false;
            try
            {
                if (transport != null)
                {
                    await _unitOfWork.Transport.CreateAsync(transport);
                    await _unitOfWork.SaveAsync();
                    refund.TransportId = transport.TransportId;
                }
                await _unitOfWork.OrderRequestRefund.UpdateAsync(refund);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                isRollback = true;
                return (false, "Cập nhật yêu cầu hoàn tiền thất bại: " + ex.Message);
            }
            finally
            {
                if (isRollback)
                    await trans.RollbackAsync();
                else
                    await trans.CommitAsync();
            }
        }    
        return (true, "Cập nhật yêu cầu hoàn tiền thành công!");
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
            Status = OrderStatusEnum.Pending,
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
            Status = o.Status.ToString(),
            OrderItems = o.OrderItems.Select(i => new OrderItemSummaryResponse
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