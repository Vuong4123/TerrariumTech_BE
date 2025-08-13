using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Order;
using TerrariumGardenTech.Common.RequestModel.Pagination;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Common.ResponseModel.Pagination;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly UnitOfWork _unitOfWork;
    private readonly IUserContextService _userContextService;

    //private readonly IUserContextService _userContextService;

    public OrderService(UnitOfWork unitOfWork,IUserContextService userContextService, ILogger<OrderService> logger  )
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
    }

    public async Task<IBusinessResult> GetAllAsync()
    {
        var orders = await _unitOfWork.Order.GetAllAsync2();

        if (orders == null || !orders.Any())
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không có đơn hàng nào!");

        var result = new List<OrderResponse>();

        foreach (var order in orders)
        {
            var orderResponse = new OrderResponse
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TotalAmount = order.TotalAmount,
                Deposit = order.Deposit,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus ?? string.Empty,
                //ShippingStatus = order.ShippingStatus ?? string.Empty,
                TransactionId = order.TransactionId,
                //PaymentMethod = order.PaymentMethod ?? string.Empty,
                OrderItems = new List<OrderItemResponse>()
                
            };

            foreach (var item in order.OrderItems)
            {
                orderResponse.OrderItems.Add(new OrderItemResponse
                {
                    OrderItemId = item.OrderItemId,
                    AccessoryId = item.AccessoryId,
                    TerrariumVariantId = item.TerrariumVariantId,
                    AccessoryQuantity = item.AccessoryQuantity,
                    TerrariumVariantQuantity = item.TerrariumVariantQuantity,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                });
            }

            result.Add(orderResponse);
        }

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách đơn hàng thành công", result);
    }


    public async Task<IBusinessResult> GetByIdAsync(int orderId)
    {
        if (orderId <= 0)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "OrderId phải là số nguyên dương!");

        var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
        if (order == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Không tìm thấy đơn hàng!");

        var orderResponse = new OrderResponse
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            AddressId = order.AddressId,
            TotalAmount = order.TotalAmount,
            Deposit = order.Deposit,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus ?? string.Empty,
            //ShippingStatus = order.ShippingStatus ?? string.Empty,
            TransactionId = order.TransactionId,
            //PaymentMethod = order.PaymentMethod ?? string.Empty,
            OrderItems = new List<OrderItemResponse>()
        };

        foreach (var item in order.OrderItems)
        {
            orderResponse.OrderItems.Add(new OrderItemResponse
            {
                OrderItemId = item.OrderItemId,
                AccessoryId = item.AccessoryId,
                TerrariumVariantId = item.TerrariumVariantId,
                AccessoryQuantity = item.AccessoryQuantity,
                TerrariumVariantQuantity = item.TerrariumVariantQuantity,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            });
        }

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy đơn hàng thành công", orderResponse);
    }


    public async Task<IBusinessResult> GetAllOrderByUserId(int userId)
    {
        var orders = await _unitOfWork.Order.FindByUserAsync(userId);

        if (orders == null || !orders.Any())
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "User chưa có đơn hàng nào!");

        var result = new List<OrderResponse>();

        foreach (var order in orders)
        {
            var orderResponse = new OrderResponse
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TotalAmount = order.TotalAmount,
                Deposit = order.Deposit,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus ?? string.Empty,
                //ShippingStatus = order.ShippingStatus ?? string.Empty,
                TransactionId = order.TransactionId,
                //PaymentMethod = order.PaymentMethod ?? string.Empty,
                OrderItems = new List<OrderItemResponse>()
            };

            foreach (var item in order.OrderItems)
            {
                orderResponse.OrderItems.Add(new OrderItemResponse
                {
                    OrderItemId = item.OrderItemId,
                    AccessoryId = item.AccessoryId,
                    TerrariumVariantId = item.TerrariumVariantId,
                    AccessoryQuantity = item.AccessoryQuantity,
                    TerrariumVariantQuantity = item.TerrariumVariantQuantity,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                });
            }

            result.Add(orderResponse);
        }

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách đơn hàng thành công", result);
    }




    //public async Task<int> CreateAsync(OrderCreateRequest request)
    //{
    //    // 1. Validate: bỏ hẳn mọi kiểm tra UnitPrice/TotalAmount
    //    ValidateCreateRequest(request);

    //    // 2. Chuẩn bị biến tính tổng tiền
    //    decimal totalAmount = 0m;
    //    var orderItems = new List<OrderItem>();

    //    // 3. Duyệt từng item, load giá gốc từ DB và tính
    //    foreach (var reqItem in request.Items)
    //    {
    //        decimal unitPrice;
    //        if (reqItem.TerrariumVariantId.HasValue)
    //        {
    //            // CHỖ SỬA: đọc giá variant
    //            var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(reqItem.TerrariumVariantId.Value);
    //            unitPrice = variant.Price;
    //        }
    //        else
    //        {
    //            // CHỖ SỬA: đọc giá accessory
    //            var acc = await _unitOfWork.Accessory.GetByIdAsync(reqItem.AccessoryId.Value);
    //            unitPrice = acc.Price;
    //        }

    //        // CHỖ SỬA: tính tổng dòng = unitPrice * quantity
    //        var lineTotal = unitPrice * reqItem.Quantity;
    //        totalAmount += lineTotal;

    //        // CHỖ SỬA: khởi tạo OrderItem với giá mới
    //        orderItems.Add(new OrderItem
    //        {
    //            AccessoryId = reqItem.AccessoryId,
    //            TerrariumVariantId = reqItem.TerrariumVariantId,
    //            TerrariumVariantQuantity = reqItem.TerrariumVariantQuantity ?? 0,
    //            AccessoryQuantity = reqItem.AccessoryQuantity ?? 0,
    //            Quantity = reqItem.Quantity,
    //            UnitPrice = unitPrice,      // giá tự tính
    //            TotalPrice = lineTotal      // tổng tự tính
    //        });
    //    }

    //    // 4. Tạo Order, gán tổng tự tính
    //    var order = new Order
    //    {
    //        UserId = request.UserId,
    //        VoucherId = request.VoucherId,
    //        Deposit = request.Deposit,
    //        TotalAmount = totalAmount,   // bỏ request.TotalAmount, dùng totalAmount
    //        OrderDate = DateTime.UtcNow,
    //        //PaymentStatus = OrderStatusEnum.Pending.ToString(),
    //        //ShippingStatus = string.IsNullOrEmpty(request.ShippingStatus) ? TransportStatusEnum.InWarehouse.ToString() : request.ShippingStatus,
    //        //PaymentMethod = request.PaymentMethod,
    //        Status = OrderStatusEnum.Pending,
    //        PaymentStatus = "Unpaid", // Mặc định là Unpaid
    //        OrderItems = orderItems
    //    };

    //    // 5. Lưu vào DB
    //    await _unitOfWork.Order.CreateAsync(order);
    //    await _unitOfWork.SaveAsync();
    //    return order.OrderId;
    //}

    public async Task<int> CreateAsync(OrderCreateRequest request)
    {
        ValidateCreateRequest(request);

        int userId = _userContextService.GetCurrentUser();

        decimal totalAmount = 0m;
        var orderItems = new List<OrderItem>();

        Voucher voucher = null;
        decimal discountAmount = 0;

        if (request.VoucherId != null && request.VoucherId != 0)
        {
            int voucherId = request.VoucherId ?? 0;
            voucher = _unitOfWork.Voucher.GetById(voucherId);

            if (voucher != null)
            {
                bool isValidVoucher = voucher.Status == VoucherStatus.Active.ToString() &&
                 voucher.ValidFrom <= System.DateTime.Now &&
                 voucher.ValidTo >= System.DateTime.Now;

                if (isValidVoucher)
                {
                    discountAmount = voucher.DiscountAmount ?? 0;
                }
            }
        }

        foreach (var reqItem in request.Items)
        {
            // Nếu có Accessory
            if (reqItem.AccessoryId.HasValue && (reqItem.AccessoryQuantity ?? 0) > 0)
            {
                var acc = await _unitOfWork.Accessory.GetByIdAsync(reqItem.AccessoryId.Value);
                int accessoryQty = reqItem.AccessoryQuantity ?? 0;
                decimal accessoryUnitPrice = acc.Price;
                decimal accessoryLineTotal = accessoryUnitPrice * accessoryQty;
                totalAmount += accessoryLineTotal;

                orderItems.Add(new OrderItem
                {
                    AccessoryId = reqItem.AccessoryId,
                    TerrariumVariantId = null,
                    AccessoryQuantity = accessoryQty,
                    TerrariumVariantQuantity = 0,
                    Quantity = accessoryQty,
                    UnitPrice = accessoryUnitPrice,
                    TotalPrice = accessoryLineTotal
                });
            }

            // Nếu có TerrariumVariant
            if (reqItem.TerrariumVariantId.HasValue && (reqItem.TerrariumVariantQuantity ?? 0) > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(reqItem.TerrariumVariantId.Value);
                int terrariumQty = reqItem.TerrariumVariantQuantity ?? 0;
                decimal variantUnitPrice = variant.Price;
                decimal variantLineTotal = variantUnitPrice * terrariumQty;
                totalAmount += variantLineTotal;

                orderItems.Add(new OrderItem
                {
                    AccessoryId = null,
                    TerrariumVariantId = reqItem.TerrariumVariantId,
                    AccessoryQuantity = 0,
                    TerrariumVariantQuantity = terrariumQty,
                    Quantity = terrariumQty,
                    UnitPrice = variantUnitPrice,
                    TotalPrice = variantLineTotal
                });
            }
        }

        totalAmount = totalAmount - discountAmount;
        var order = new Order
        {
            UserId = userId,
            VoucherId = request.VoucherId,
            AddressId = request.AddressId,
            Deposit = request.Deposit,
            TotalAmount = totalAmount,
            OrderDate = System.DateTime.UtcNow,
            Status = OrderStatusEnum.Pending,
            PaymentStatus = "Unpaid",
            OrderItems = orderItems,
            
        };

        try
        {
            await _unitOfWork.Order.CreateAsync(order);
            await _unitOfWork.SaveAsync();
            return order.OrderId;
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException != null)
                msg += " | INNER: " + ex.InnerException.Message;

            // Log vào hệ thống để debug (nếu có _logger)
            _logger?.LogError(ex, $"Order creation failed: {msg}");

            // Có thể trả về hoặc throw để FE/debug nhìn thấy lỗi thật
            throw new Exception("Order create error: " + msg, ex);
        }
    }







    public async Task<bool> UpdateStatusAsync(int id, OrderStatusEnum status)
    {
        if (id <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));

        // Optional: kiểm tra giá trị enum có hợp lệ hay không
        if (!Enum.IsDefined(typeof(OrderStatusEnum), status))
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

        // Specify the type arguments explicitly to resolve CS0411
        return orders.Select(order => ToResponse(order, _unitOfWork));
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
            PaymentDate = System.DateTime.UtcNow
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
            RequestDate = System.DateTime.Today            
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
                CreatedDate = System.DateTime.Now,
                CreatedBy = currentUser.Username
            };
        }
        using (var trans = await _unitOfWork.OrderRequestRefund.BeginTransactionAsync())
        {
            refund.Status = request.Status;
            refund.ReasonModified = request.Reason;
            refund.UserModified = currentUserId;
            refund.LastModifiedDate = System.DateTime.Now;
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

    public async Task<IBusinessResult> GetAllWithPaginationAsync(PaginationRequest request)
    {
        try
        {
            var page = request.ValidPage;
            var pageSize = request.ValidPageSize;

            var (orders, totalCount) = await _unitOfWork.Order.GetAllWithPaginationAsync(page, pageSize);

            if (!orders.Any())
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không có đơn hàng nào!",
                    new PaginationResponse<OrderResponse>
                    {
                        Items = new List<OrderResponse>(),
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalItems = 0,
                        TotalPages = 0
                    });
            }

            var result = new List<OrderResponse>();

            foreach (var order in orders)
            {
                var orderResponse = new OrderResponse
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    AddressId = order.AddressId,
                    TotalAmount = order.TotalAmount,
                    Deposit = order.Deposit,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus ?? string.Empty,
                    TransactionId = order.TransactionId,
                    OrderItems = new List<OrderItemResponse>()
                };

                foreach (var item in order.OrderItems ?? Enumerable.Empty<OrderItem>())
                {
                    orderResponse.OrderItems.Add(new OrderItemResponse
                    {
                        OrderItemId = item.OrderItemId,
                        AccessoryId = item.AccessoryId,
                        TerrariumVariantId = item.TerrariumVariantId,
                        AccessoryQuantity = item.AccessoryQuantity,
                        TerrariumVariantQuantity = item.TerrariumVariantQuantity,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }

                result.Add(orderResponse);
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var paginationResponse = new PaginationResponse<OrderResponse>
            {
                Items = result,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = totalPages
            };

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách đơn hàng thành công", paginationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders with pagination");
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetByUserWithPaginationAsync(int userId, PaginationRequest request)
    {
        try
        {
            if (userId <= 0)
                return new BusinessResult(Const.BAD_REQUEST_CODE, "UserId phải là số nguyên dương!");

            var page = request.ValidPage;
            var pageSize = request.ValidPageSize;

            var (orders, totalCount) = await _unitOfWork.Order.GetByUserWithPaginationAsync(userId, page, pageSize);

            if (!orders.Any())
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "User chưa có đơn hàng nào!",
                    new PaginationResponse<OrderResponse>
                    {
                        Items = new List<OrderResponse>(),
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalItems = 0,
                        TotalPages = 0
                    });
            }

            var result = new List<OrderResponse>();

            foreach (var order in orders)
            {
                var orderResponse = new OrderResponse
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    AddressId = order.AddressId,
                    TotalAmount = order.TotalAmount,
                    Deposit = order.Deposit,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    PaymentStatus = order.PaymentStatus ?? string.Empty,
                    TransactionId = order.TransactionId,
                    OrderItems = new List<OrderItemResponse>()
                };

                foreach (var item in order.OrderItems ?? Enumerable.Empty<OrderItem>())
                {
                    orderResponse.OrderItems.Add(new OrderItemResponse
                    {
                        OrderItemId = item.OrderItemId,
                        AccessoryId = item.AccessoryId,
                        TerrariumVariantId = item.TerrariumVariantId,
                        AccessoryQuantity = item.AccessoryQuantity,
                        TerrariumVariantQuantity = item.TerrariumVariantQuantity,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }

                result.Add(orderResponse);
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var paginationResponse = new PaginationResponse<OrderResponse>
            {
                Items = result,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = totalPages
            };

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách đơn hàng theo user thành công", paginationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by user with pagination");
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    #region Helpers

    //private static void ValidateCreateRequest(OrderCreateRequest r)
    //{
    //    if (r.UserId <= 0)
    //        throw new ArgumentException("UserId phải là số nguyên dương.", nameof(r.UserId));

    //    if (r.Items == null || !r.Items.Any())
    //        throw new ArgumentException("Phải có ít nhất một item trong đơn hàng.", nameof(r.Items));

    //    foreach (var item in r.Items)
    //    {
    //        if (item.Quantity <= 0)
    //            throw new ArgumentException("Quantity của item phải lớn hơn 0.", nameof(item.Quantity));
    //        if (item.AccessoryId == null && item.TerrariumVariantId == null)
    //            throw new ArgumentException("Item phải có AccessoryId hoặc TerrariumVariantId.", nameof(r.Items));
    //    }
    //}
    private static void ValidateCreateRequest(OrderCreateRequest r)
    {
        // KHÔNG cần check UserId
        if (r.Items == null || !r.Items.Any())
            throw new ArgumentException("Phải có ít nhất một item trong đơn hàng.", nameof(r.Items));

        foreach (var item in r.Items)
        {
            // KHÔNG cần check Quantity
            if ((item.AccessoryId == null && item.TerrariumVariantId == null)
                || ((item.AccessoryQuantity ?? 0) <= 0 && (item.TerrariumVariantQuantity ?? 0) <= 0))
                throw new ArgumentException("Item phải có AccessoryId hoặc TerrariumVariantId và quantity > 0.", nameof(r.Items));
        }
    }



    // CHUYỂN thành async để gọi DB bên trong
    private async Task<Order> MapToEntityAsync(OrderCreateRequest r, int userId)
    {
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0m;

        foreach (var reqItem in r.Items)
        {
            if (reqItem.AccessoryId.HasValue && (reqItem.AccessoryQuantity ?? 0) > 0)
            {
                var acc = await _unitOfWork.Accessory.GetByIdAsync(reqItem.AccessoryId.Value);
                int qty = reqItem.AccessoryQuantity ?? 0;
                decimal unitPrice = acc.Price;
                var lineTotal = unitPrice * qty;
                totalAmount += lineTotal;

                orderItems.Add(new OrderItem
                {
                    AccessoryId = reqItem.AccessoryId,
                    TerrariumVariantId = null,
                    AccessoryQuantity = qty,
                    TerrariumVariantQuantity = 0,
                    Quantity = qty,
                    UnitPrice = unitPrice,
                    TotalPrice = lineTotal
                });
            }
            if (reqItem.TerrariumVariantId.HasValue && (reqItem.TerrariumVariantQuantity ?? 0) > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(reqItem.TerrariumVariantId.Value);
                int qty = reqItem.TerrariumVariantQuantity ?? 0;
                decimal unitPrice = variant.Price;
                var lineTotal = unitPrice * qty;
                totalAmount += lineTotal;

                orderItems.Add(new OrderItem
                {
                    AccessoryId = null,
                    TerrariumVariantId = reqItem.TerrariumVariantId,
                    AccessoryQuantity = 0,
                    TerrariumVariantQuantity = qty,
                    Quantity = qty,
                    UnitPrice = unitPrice,
                    TotalPrice = lineTotal
                });
            }
        }

        var order = new Order
        {
            UserId = userId, // Truyền từ controller hoặc context
            VoucherId = r.VoucherId,
            Deposit = r.Deposit,
            TotalAmount = totalAmount,
            OrderDate = System.DateTime.UtcNow,
            Status = OrderStatusEnum.Pending,
            OrderItems = orderItems
        };

        return order;
    }



    private static OrderResponse ToResponse(Order o, UnitOfWork unitOfWork)
    {
        decimal discountAmount = 0;
        if (o.VoucherId != null && o.VoucherId > 0)
        {
            var voucher = unitOfWork.Voucher.GetByIdAsync(o.VoucherId.Value).Result;
            if (voucher != null)
            {
                discountAmount = voucher.DiscountAmount ?? 0;
            }
        }

        return new OrderResponse
        {
            OrderId = o.OrderId,
            UserId = o.UserId,
            TotalAmount = o.TotalAmount,
            Deposit = o.Deposit,
            OrderDate = o.OrderDate,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            DiscountAmount = discountAmount,
            TransactionId = o.TransactionId,
            PaymentMethod = o.Payment != null && o.Payment.Count > 0 ? o.Payment.First().PaymentMethod : "",
            OrderItems = o.OrderItems.Select(i => new OrderItemResponse
            {
                OrderItemId = i.OrderItemId,
                AccessoryId = i.AccessoryId,
                TerrariumVariantId = i.TerrariumVariantId,
                Quantity = i.Quantity ?? 0,
                UnitPrice = i.UnitPrice ?? 0,
                TotalPrice = i.TotalPrice ?? 0,
                AccessoryQuantity = i.AccessoryQuantity ?? 0,
                TerrariumVariantQuantity = i.TerrariumVariantQuantity ?? 0
            }).ToList()
        };
    }
    #endregion
}