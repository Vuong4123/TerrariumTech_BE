using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.ResponseModel.Address;
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
        var orders = await _unitOfWork.Order.GetAllAsync(); // Gọi từ UnitOfWork
        return orders.Select(ToResponse);
    }

    public async Task<OrderResponse?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));

        var order = await _unitOfWork.Order.GetOrderbyIdAsync(id); 
        if (order == null)
        {
            _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId}", id);
            return null;
        }

        return ToResponse(order);
    }

    public async Task<IBusinessResult> GetAllOrderByUserId(int userId)
    {
        // Lấy các địa chỉ theo userId từ cơ sở dữ liệu
        var orderrrr = await _unitOfWork.Address.GetByUserIdAsync(userId);

        // Kiểm tra nếu không có dữ liệu
        if (orderrrr == null || !orderrrr.Any())
        {
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }

        // Trả về kết quả với dữ liệu đã ánh xạ
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, orderrrr);
    }



    public async Task<int> CreateAsync(OrderCreateRequest request)
    {
        // 1. Validate: bỏ hẳn mọi kiểm tra UnitPrice/TotalAmount
        ValidateCreateRequest(request);

        // 2. Chuẩn bị biến tính tổng tiền
        decimal totalAmount = 0m;
        var orderItems = new List<OrderItem>();

        // 3. Duyệt từng item, load giá gốc từ DB và tính
        foreach (var reqItem in request.Items)
        {
            decimal unitPrice;
            if (reqItem.TerrariumVariantId.HasValue)
            {
                // CHỖ SỬA: đọc giá variant
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(reqItem.TerrariumVariantId.Value);
                unitPrice = variant.Price;
            }
            else
            {
                // CHỖ SỬA: đọc giá accessory
                var acc = await _unitOfWork.Accessory.GetByIdAsync(reqItem.AccessoryId.Value);
                unitPrice = acc.Price;
            }

            // CHỖ SỬA: tính tổng dòng = unitPrice * quantity
            var lineTotal = unitPrice * reqItem.Quantity;
            totalAmount += lineTotal;

            // CHỖ SỬA: khởi tạo OrderItem với giá mới
            orderItems.Add(new OrderItem
            {
                AccessoryId = reqItem.AccessoryId,
                TerrariumVariantId = reqItem.TerrariumVariantId,
                Quantity = reqItem.Quantity,
                UnitPrice = unitPrice,      // giá tự tính
                TotalPrice = lineTotal      // tổng tự tính
            });
        }

        // 4. Tạo Order, gán tổng tự tính
        var order = new Order
        {
            UserId = request.UserId,
            VoucherId = request.VoucherId,
            Deposit = request.Deposit,
            TotalAmount = totalAmount,   // bỏ request.TotalAmount, dùng totalAmount
            OrderDate = DateTime.UtcNow,
            Status = OrderStatusEnum.Pending,
            //PaymentStatus = "Unpaid", // Mặc định là Unpaid
            OrderItems = orderItems
        };

        // 5. Lưu vào DB
        await _unitOfWork.Order.CreateAsync(order);
        await _unitOfWork.SaveAsync();
        return order.OrderId;
    }



    public async Task<bool> UpdateStatusAsync(int id, OrderStatusEnum status)
    {
        if (id <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));

        // Optional: kiểm tra giá trị enum có hợp lệ hay không
        if (!Enum.IsDefined(typeof(OrderStatus), status))
            throw new ArgumentException("Status không hợp lệ.", nameof(status));

        var order = await _unitOfWork.Order.GetByIdAsync(id);

        if (order == null)
        {
            _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId} để cập nhật", id);
            return false;
        }
        //OrderStatusEnum enumStatus;
        //if (Enum.TryParse<OrderStatusEnum>(status, true, out enumStatus))
        //{
        //    _logger.LogWarning("Mã trạng thái đơn hàng với ID {OderId} không chính xác", id);
        //    return false;
        //}    

        try
        {
            order.Status = status;
            await _unitOfWork.Order.UpdateAsync(order); // Gọi từ UnitOfWork
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



    public async Task<IEnumerable<OrderResponse>> GetByUserAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId phải là số nguyên dương.", nameof(userId));

        // OrderRepository đã có sẵn phương thức FindByUserAsync
        var orders = await _unitOfWork.Order.FindByUserAsync(userId);
        return orders.Select(ToResponse);
    }


    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));

        var order = await _unitOfWork.Order.GetByIdAsync(id); // Gọi từ UnitOfWork
        if (order == null)
        {
            _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId} để xóa", id);
            return false;
        }

        try
        {
            await _unitOfWork.Order.RemoveAsync(order); // Gọi từ UnitOfWork
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

        // 1. Validate input
        if (orderId <= 0)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "OrderId không hợp lệ.", null);
        if (string.IsNullOrWhiteSpace(paymentMethod))
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Payment method là bắt buộc.", null);

        // 2. Lấy order

        var order = await _unitOfWork.Order.GetByIdAsync(orderId);
        if (order == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Order does not exist", null);


        // 3. Đã thanh toán rồi thì thôi
        if (order.PaymentStatus == "Paid")
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Order already paid", null);

        // 4. Tính số tiền còn phải trả (trừ deposit nếu có)
        decimal deposit = order.Deposit ?? 0m;
        decimal due = order.TotalAmount - deposit;
        if (due <= 0)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Nothing to pay—order already covered by deposit.", null);

        // 5. Cập nhật trạng thái thanh toán
        order.PaymentStatus = "Paid";

        // 6. Tạo record giao dịch thanh toán
        var payment = new Payment
        {
            OrderId = order.OrderId,
            PaymentMethod = paymentMethod.Trim(),
            PaymentAmount = due,

            PaymentDate = DateTime.UtcNow
        };

        try
        {

            // 7. Lưu thay đổi
            await _unitOfWork.Order.UpdateAsync(order);
            await _unitOfWork.Payment.AddAsync(payment);
            await _unitOfWork.SaveAsync();


            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Payment successful", null);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Checkout failed for OrderId {OrderId}", orderId);

            return new BusinessResult(Const.ERROR_EXCEPTION, "Payment failed, please try again", null);
        }
    }


    public async Task<(bool, string)> RequestRefundAsync(CreateRefundRequest request, int currentUserId)
    {
        var order = await _unitOfWork.Order.DbSet().FirstOrDefaultAsync(x => x.OrderId == request.OrderId && x.UserId == currentUserId);
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

        var order = await _unitOfWork.Order.FindOneAsync(x => x.OrderId == refund.OrderId);
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
            if (item.AccessoryId == null && item.TerrariumVariantId == null)
                throw new ArgumentException("Item phải có AccessoryId hoặc TerrariumVariantId.", nameof(r.Items));
        }
    }


    // CHUYỂN thành async để gọi DB bên trong
    private async Task<Order> MapToEntityAsync(OrderCreateRequest r)
    {
        // 1. Tạo list chứa OrderItem
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0m;

        foreach (var reqItem in r.Items)
        {
            // 2. Lấy price từ database
            decimal unitPrice;
            if (reqItem.TerrariumVariantId.HasValue)
            {
                // CHỖ SỬA: lấy giá variant
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(reqItem.TerrariumVariantId.Value);
                unitPrice = variant.Price;
            }
            else
            {
                // CHỖ SỬA: lấy giá accessory
                var acc = await _unitOfWork.Accessory.GetByIdAsync(reqItem.AccessoryId.Value);
                unitPrice = acc.Price;
            }

            // 3. Tính total của dòng
            var lineTotal = unitPrice * reqItem.Quantity;
            totalAmount += lineTotal;

            // 4. Khởi tạo OrderItem với giá tự tính
            orderItems.Add(new OrderItem
            {
                AccessoryId = reqItem.AccessoryId,
                TerrariumVariantId = reqItem.TerrariumVariantId,
                Quantity = reqItem.Quantity,
                UnitPrice = unitPrice,      // giá tự fetch
                TotalPrice = lineTotal       // tính ra
            });
        }

        // 5. Tạo hẳn entity Order với tổng tự tính
        var order = new Order
        {
            UserId = r.UserId,
            VoucherId = r.VoucherId,
            Deposit = r.Deposit,
            TotalAmount = totalAmount,             // gán total tự tính
            OrderDate = DateTime.UtcNow,
            Status = OrderStatusEnum.Pending,
            OrderItems = orderItems
        };

        return order;
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
            OrderItems = o.OrderItems.Select(i => new OrderItemResponse
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