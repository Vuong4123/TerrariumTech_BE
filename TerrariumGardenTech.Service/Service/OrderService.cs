using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.Xml;
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
using static TerrariumGardenTech.Common.Enums.CommonData;

namespace TerrariumGardenTech.Service.Service;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly UnitOfWork _unitOfWork;
    private readonly IUserContextService _userContextService;
    private readonly IWalletServices _walletService;

    //private readonly IUserContextService _userContextService;

    public OrderService(UnitOfWork unitOfWork,IUserContextService userContextService, ILogger<OrderService> logger, IWalletServices walletServices)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _walletService = walletServices ?? throw new ArgumentNullException(nameof(walletServices));
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
                    ComboId = item.ComboId ?? 0,
                    ItemType = item.ItemType,
                    OrderItemId = item.OrderItemId,
                    TerrariumId = item.TerrariumId,
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
                ComboId = item.ComboId ?? 0,
                OrderItemId = item.OrderItemId,
                ItemType = item.ItemType,
                TerrariumId = item.TerrariumId,
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
                    ComboId = item.ComboId ?? 0,
                    ItemType = item.ItemType,
                    OrderItemId = item.OrderItemId,
                    TerrariumId = item.TerrariumId,
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
    //        //PaymentStatus = OrderStatusData.Pending.ToString(),
    //        //ShippingStatus = string.IsNullOrEmpty(request.ShippingStatus) ? TransportStatusEnum.InWarehouse.ToString() : request.ShippingStatus,
    //        //PaymentMethod = request.PaymentMethod,
    //        Status = OrderStatusData.Pending,
    //        PaymentStatus = "Unpaid", // Mặc định là Unpaid
    //        OrderItems = orderItems
    //    };

    //    // 5. Lưu vào DB
    //    await _unitOfWork.Order.CreateAsync(order);
    //    await _unitOfWork.SaveAsync();
    //    return order.OrderId;
    //}
    public async Task<IBusinessResult> CancelOrderAsync(int orderId, int userId, CancelOrderRequest request)
    {
        try
        {
            // 1. Lấy thông tin đơn hàng
            var order = await _unitOfWork.Order.GetByIdAsync(orderId);
            if (order == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy đơn hàng");

            // 2. Kiểm tra quyền
            if (order.UserId != userId)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Bạn không có quyền hủy đơn hàng này");

            // 3. Validate trạng thái có thể hủy
            var cancellableStatuses = new[] {
            OrderStatusData.Pending,
            OrderStatusData.Processing
        };

            if (!cancellableStatuses.Contains(order.Status))
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE,
                    $"Không thể hủy đơn hàng ở trạng thái {order.Status}. Chỉ có thể hủy đơn ở trạng thái Pending hoặc Processing");
            }

            // 4. Validate lý do hủy
            if (string.IsNullOrWhiteSpace(request.CancelReason))
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Vui lòng nhập lý do hủy đơn");

            // 5. Cập nhật trạng thái đơn hàng
            order.Status = OrderStatusData.Cancel;
            order.PaymentStatus = "Unpaid";
            // 7. Xử lý hoàn tiền vào ví (nếu đã thanh toán)
            decimal refundAmount = 0;
            if (order.PaymentStatus == "Paid" || order.Deposit > 0)
            {
                refundAmount = await ProcessRefundToWalletAsync(order, userId, request);
            }

            // 8. Hoàn lại stock cho sản phẩm
            await RestoreStockForOrderItemsAsync(orderId);

            // 9. Gửi notification
            await SendCancelNotificationAsync(order, request.CancelReason);

            // 10. Save changes
            await _unitOfWork.Order.UpdateAsync(order);
            await _unitOfWork.SaveAsync();

            var response = new CancelOrderResponse
            {
                OrderId = orderId,
                Status = order.Status.ToString(),
                CancelledAt = DateTime.UtcNow,
                CancelReason = request.CancelReason,
                RefundAmount = refundAmount,
                RefundStatus = refundAmount > 0 ? "Đã hoàn tiền vào ví" : "Không có tiền cần hoàn",
                Message = "Hủy đơn hàng thành công"
            };

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Hủy đơn hàng thành công", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", orderId, userId);
            return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi hủy đơn hàng: {ex.Message}");
        }
    }
    #region Helper
    // Xử lý hoàn tiền vào ví
    private async Task<decimal> ProcessRefundToWalletAsync(Order order, int userId, CancelOrderRequest request)
    {
        // Tính số tiền cần hoàn
        decimal refundAmount = 0;

        // Nếu đã thanh toán toàn bộ
        if (order.PaymentStatus == "Paid")
        {
            refundAmount = order.TotalAmount - (order.DiscountAmount ?? 0);
        }
        // Nếu chỉ đặt cọc
        else if (order.Deposit > 0)
        {
            refundAmount = order.Deposit.Value;
        }

        if (refundAmount > 0)
        {
            // Lấy ví của user
            var wallet = await _unitOfWork.Wallet.GetByUserIdAndTypeAsync(userId, "User");
            if (wallet == null)
            {
                // Tạo ví mới nếu chưa có
                wallet = new Wallet
                {
                    UserId = userId,
                    Balance = 0
                };
                await _unitOfWork.Wallet.CreateAsync(wallet);
            }

            // Cộng tiền vào ví
            wallet.Balance += refundAmount;
            await _unitOfWork.Wallet.UpdateAsync(wallet);

            // Tạo transaction history
            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Type = "Refund",
                Amount = refundAmount,
                OrderId = order.OrderId,
            };
            await _unitOfWork.WalletTransactionRepository.CreateAsync(transaction);

            // Tạo refund record
            var refund = new OrderRequestRefund
            {
                OrderId = order.OrderId,
                RefundAmount = refundAmount,
                Reason = request.CancelReason,
                Status = OrderStatusData.Pending,
                RequestDate = DateTime.UtcNow
            };
            await _unitOfWork.OrderRequestRefund.CreateAsync(refund);
        }

        return refundAmount;
    }

    // Hoàn lại stock cho sản phẩm
    private async Task RestoreStockForOrderItemsAsync(int orderId)
    {
        var order = _unitOfWork.Order.GetById(orderId);

        foreach (var item in order.OrderItems)
        {
            // Hoàn stock cho accessory
            if (item.AccessoryId.HasValue && item.AccessoryQuantity > 0)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                if (accessory != null)
                {
                    accessory.StockQuantity += item.AccessoryQuantity.Value;
                    await _unitOfWork.Accessory.UpdateAsync(accessory);
                }
            }

            // Hoàn stock cho terrarium variant
            if (item.TerrariumVariantId.HasValue && item.TerrariumVariantQuantity > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                if (variant != null)
                {
                    variant.StockQuantity += item.TerrariumVariantQuantity.Value;
                    await _unitOfWork.TerrariumVariant.UpdateAsync(variant);
                }
            }
        }
    }

    // Gửi notification
    private async Task SendCancelNotificationAsync(Order order, string reason)
    {
        // Email notification
        var emailContent = $@"
        <h3>Đơn hàng #{order.OrderId} đã được hủy</h3>
        <p>Lý do: {reason}</p>
        <p>Số tiền sẽ được hoàn vào ví của bạn trong vòng 24h.</p>
    ";

        // Gửi email (implement theo email service của bạn)
        // await _emailService.SendEmailAsync(order.User.Email, "Thông báo hủy đơn hàng", emailContent);

        // In-app notification
        var notification = new Notification
        {
            UserId = order.UserId,
            Title = "Đơn hàng đã được hủy",
            Message = $"Đơn hàng #{order.OrderId} đã được hủy thành công",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Notification.CreateAsync(notification);
    }

    #endregion
    public async Task<IBusinessResult> AcceptRefundRequestAsync(int refundId, int staffId, AcceptRefundRequest request)
    {
        try
        {
            // 1. Lấy thông tin refund request
            var refundRequest = await _unitOfWork.OrderRequestRefund.GetByIdAsync(refundId);
            if (refundRequest == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy yêu cầu hoàn tiền");

            // 2. Kiểm tra trạng thái
            if (refundRequest.Status != OrderStatusData.Pending)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE,
                    $"Yêu cầu hoàn tiền đã được xử lý trước đó với trạng thái: {refundRequest.Status}");
            }

            // 3. Lấy thông tin order
            var order = await _unitOfWork.Order.GetByIdAsync(refundRequest.OrderId);
            if (order == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy đơn hàng liên quan");

            // 4. Xử lý approve/reject
            if (request.IsApproved)
            {
                // APPROVE - Thực hiện hoàn tiền
                refundRequest.Status = OrderStatusData.Approved;
                refundRequest.UserModified = staffId;
                refundRequest.LastModifiedDate = DateTime.UtcNow;

                // Thực hiện hoàn tiền vào ví
                var refundResult = await ProcessActualRefundAsync(order, refundRequest);
                if (!refundResult.Success)
                {
                    return new BusinessResult(Const.FAIL_UPDATE_CODE,
                        $"Lỗi khi hoàn tiền: {refundResult.Message}");
                }

                // Cập nhật trạng thái order nếu cần
                if (order.Status == OrderStatusData.Cancel)
                {
                    order.Status = OrderStatusData.Refunded;
                }

                // Gửi notification cho user
                await SendRefundApprovedNotificationAsync(order, refundRequest);
            }
            else
            {
                // REJECT - Từ chối hoàn tiền
                if (string.IsNullOrWhiteSpace(request.RejectionReason))
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Vui lòng nhập lý do từ chối");

                refundRequest.Status = OrderStatusData.Rejected;
                refundRequest.UserModified = staffId;
                refundRequest.LastModifiedDate = DateTime.UtcNow;
                refundRequest.Notes = request.RejectionReason;

                // Gửi notification cho user
                await SendRefundRejectedNotificationAsync(order, refundRequest, request.RejectionReason);
            }

            // 5. Save changes
            await _unitOfWork.OrderRequestRefund.UpdateAsync(refundRequest);
            await _unitOfWork.Order.UpdateAsync(order);
            await _unitOfWork.SaveAsync();

            var response = new AcceptRefundResponse
            {
                RefundId = refundId,
                OrderId = order.OrderId,
                RefundStatus = refundRequest.Status,
                RefundAmount = refundRequest.RefundAmount ?? 0,
                ProcessedAt = refundRequest.LastModifiedDate,
                ProcessedBy = staffId,
                IsApproved = request.IsApproved,
                Message = request.RejectionReason
            };

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, response.Message, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting refund request {RefundId}", refundId);
            return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi xử lý yêu cầu hoàn tiền: {ex.Message}");
        }
    }
    #region Helper for Refund Approval
    // Thực hiện hoàn tiền thực tế
    private async Task<(bool Success, string Message)> ProcessActualRefundAsync(Order order, OrderRequestRefund refundRequest)
    {
        try
        {
            // Lấy ví của user
            var wallet = await _unitOfWork.Wallet.GetByUserIdAndTypeAsync(order.UserId, "User");
            if (wallet == null)
            {
                // Tạo ví mới nếu chưa có
                wallet = new Wallet
                {
                    UserId = order.UserId,
                    Balance = 0,
                    WalletType = "User"
                };
                await _unitOfWork.Wallet.CreateAsync(wallet);
                await _unitOfWork.SaveAsync();
            }

            // Cộng tiền vào ví
            wallet.Balance += refundRequest.RefundAmount ?? 0;
            await _unitOfWork.Wallet.UpdateAsync(wallet);

            // Tạo transaction history
            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Type = "Refund",
                Amount = refundRequest.RefundAmount ?? 0,
                OrderId = order.OrderId
            };
            await _unitOfWork.WalletTransactionRepository.CreateAsync(transaction);

            return (true, "Hoàn tiền thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for order {OrderId}", order.OrderId);
            return (false, ex.Message);
        }
    }
    public async Task<IBusinessResult> GetPendingRefundRequestsAsync()
    {
        try
        {
            var pendingRefunds = await _unitOfWork.OrderRequestRefund
                .GetAllWithStatus(OrderStatusData.Pending);

            var response = new List<RefundRequestDto>();

            foreach (var refund in pendingRefunds)
            {
                var order = await _unitOfWork.Order.GetByIdAsync(refund.OrderId);
                var user = await _unitOfWork.User.GetByIdAsync(order.UserId);

                response.Add(new RefundRequestDto
                {
                    RefundId = refund.RequestRefundId,
                    OrderId = refund.OrderId,
                    UserId = order.UserId,
                    UserEmail = user?.Email,
                    RefundAmount = refund.RefundAmount ?? 0,
                    Images = refund.Images,
                    Reason = refund.Reason,
                    RequestDate = refund.RequestDate,
                    RefundStatus = refund.Status,
                    OrderStatus = order.Status.ToString()
                });
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách thành công", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending refund requests");
            return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi: {ex.Message}");
        }
    }

    // Gửi notification khi duyệt hoàn tiền
    private async Task SendRefundApprovedNotificationAsync(Order order, OrderRequestRefund refundRequest)
    {
        var notification = new Notification
        {
            UserId = order.UserId,
            Title = "Yêu cầu hoàn tiền được chấp nhận",
            Message = $"Yêu cầu hoàn tiền {refundRequest.RefundAmount:C} cho đơn hàng #{order.OrderId} đã được duyệt. Tiền đã được chuyển vào ví của bạn.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Notification.CreateAsync(notification);
    }

    // Gửi notification khi từ chối hoàn tiền
    private async Task SendRefundRejectedNotificationAsync(Order order, OrderRequestRefund refundRequest, string reason)
    {
        var notification = new Notification
        {
            UserId = order.UserId,
            Title = "Yêu cầu hoàn tiền bị từ chối",
            Message = $"Yêu cầu hoàn tiền cho đơn hàng #{order.OrderId} đã bị từ chối. Lý do: {reason}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Notification.CreateAsync(notification);
    }

    #endregion

    public async Task<int> CreateAsync(OrderCreateRequest request)
    {
        if (request.ComboId <= 0)
        {
            ValidateCreateRequest(request);
        }
        if (request.VoucherId == 0) request.VoucherId = null;

        int userId = _userContextService.GetCurrentUser();
        decimal totalAmount = 0m;
        var orderItems = new List<OrderItem>();

        int? safeVoucherId = null;
        decimal discountAmount = 0m;

        if (request.VoucherId.HasValue && request.VoucherId.Value > 0)
        {
            var voucher = await _unitOfWork.Voucher.GetByIdAsync(request.VoucherId.Value);
            if (voucher != null)
            {
                bool isValid =
                    voucher.Status == VoucherStatus.Active.ToString() &&
                    voucher.ValidFrom <= DateTime.UtcNow &&
                    voucher.ValidTo >= DateTime.UtcNow;

                if (isValid)
                {
                    safeVoucherId = voucher.VoucherId;
                    discountAmount = voucher.DiscountAmount ?? 0m;
                }
            }
        }

        foreach (var reqItem in request.Items)
        {
            if (reqItem.AccessoryId.HasValue && (reqItem.AccessoryQuantity ?? 0) > 0)
            {
                var acc = await _unitOfWork.Accessory.GetByIdAsync(reqItem.AccessoryId.Value);
                int qty = reqItem.AccessoryQuantity ?? 0;
                decimal unit = acc.Price;
                decimal line = unit * qty;
                totalAmount += line;

                orderItems.Add(new OrderItem
                {
                    AccessoryId = reqItem.AccessoryId,
                    TerrariumId = reqItem.TerrariumId,
                    TerrariumVariantId = null,
                    AccessoryQuantity = qty,
                    TerrariumVariantQuantity = 0,
                    Quantity = qty,
                    UnitPrice = unit,
                    ItemType = reqItem.ItemType,
                    TotalPrice = line
                });
            }

            if (reqItem.TerrariumVariantId.HasValue && (reqItem.TerrariumVariantQuantity ?? 0) > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(reqItem.TerrariumVariantId.Value);
                int qty = reqItem.TerrariumVariantQuantity ?? 0;
                decimal unit = variant.Price;
                decimal line = unit * qty;
                totalAmount += line;

                orderItems.Add(new OrderItem
                {
                    AccessoryId = null,
                    TerrariumId = reqItem.TerrariumId,
                    TerrariumVariantId = reqItem.TerrariumVariantId,
                    AccessoryQuantity = 0,
                    TerrariumVariantQuantity = qty,
                    Quantity = qty,
                    UnitPrice = unit,
                    ItemType = reqItem.ItemType,
                    TotalPrice = line
                });
            }
            if (request.ComboId.HasValue && (request.ComboId ?? 0) > 0)
            {
                var combo = await _unitOfWork.Combo.GetByIdAsync(request.ComboId ?? 0);
                if(combo == null) { throw new ArgumentException("Combo chưa tồn tại"); };
                foreach (var com in combo.ComboItems)
                {
                    orderItems.Add(new OrderItem
                    {
                        ComboId = request.ComboId,
                        AccessoryId = com.AccessoryId,
                        TerrariumId = reqItem.TerrariumId,
                        TerrariumVariantId = com.TerrariumVariantId,
                        AccessoryQuantity = 1,
                        TerrariumVariantQuantity = 1,
                        Quantity = 1,
                        UnitPrice = 0,
                        ItemType = reqItem.ItemType,
                        TotalPrice = request.TotalAmount
                    });
                }
            }
        }

        totalAmount -= discountAmount;
        if (totalAmount < 0) totalAmount = 0; // tránh âm

        var order = new Order
        {
            UserId = userId,
            VoucherId = safeVoucherId,     // chỉ set khi hợp lệ
            AddressId = request.AddressId,
            Deposit = request.Deposit,
            TotalAmount = totalAmount,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatusData.Pending,
            PaymentStatus = "Unpaid",
            OrderItems = orderItems,
        };

        try
        {
            await _unitOfWork.Order.CreateAsync(order);
            //await _unitOfWork.SaveAsync();            // <-- cần commit để lấy ID
            return order.OrderId;
        }
        catch (Exception ex)
        {
            var msg = ex.Message + (ex.InnerException != null ? " | INNER: " + ex.InnerException.Message : "");
            _logger?.LogError(ex, $"Order creation failed: {msg}");
            throw new Exception("Order create error: " + msg, ex);
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        if (id <= 0)
            throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));
        var order = await _unitOfWork.Order.GetByIdAsync(id);

        if (order == null)
        {
            _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId} để cập nhật", id);
            return false;
        }
        //OrderStatusData enumStatus;
        //if (Enum.TryParse<OrderStatusData>(status, true, out enumStatus))
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


    public async Task<(bool, string)> RequestRefundAsync(RefundRequest request, int currentUserId)
    {
        var order = await _unitOfWork.Order.DbSet()
            .Include(x => x.OrderItems)
            .Include(r => r.Refunds)
            .FirstOrDefaultAsync(x => x.OrderId == request.OrderId && x.UserId == currentUserId);
        if (order == null)
            return (false, "Không tìm thấy thông tin hóa đơn!");

        var orderItems = order.OrderItems.ToList();
        if (order.Status != OrderStatusData.Completed)
            return (false, "Đơn hàng này không cho phép hoàn tiền!");

        if (orderItems == null || !orderItems.Any())
            return (false, "Vui lòng chọn sản phẩm cần hoàn tiền!");

        if (orderItems.Any(x => x.Quantity <= 0))
            return (false, "Số lượng sản phẩm yêu cầu hoàn tiền phải lớn hơn 0!");

        if (string.IsNullOrEmpty(request.Reason))
            return (false, "Vui lòng nhập đầy đủ lý do hoàn tiền!");

        if (orderItems.Any(x => !order.OrderItems.Any(i => i.OrderItemId == x.OrderItemId)))
            return (false, "Danh sách sản phẩm cần hoàn tiền không chính xác!");

        if (orderItems.Any(x => order.OrderItems.Any(i => x.OrderItemId == x.OrderItemId && i.Quantity < x.Quantity)))
            return (false, "Số lượng sản phẩm yêu cầu hoàn tiền vượt quá số lượng đã đặt trong đơn hàng!");
        
        if(order.Refunds.Count() > 0)
            return (false, "Đơn hàng này đang xử lý hoàn tiền !");
        var existedRefundItems = await _unitOfWork.OrderRequestRefund.DbSet()
            .Include(x => x.Items).Where(x => x.OrderId == order.OrderId && x.Status == CommonData.OrderStatusData.Approved)
            .SelectMany(x => x.Items).Where(x => x.IsApproved == true).ToArrayAsync();
        if (existedRefundItems.Any())
        {
            var checkQuantity = orderItems.Select(x =>
            {
                // Số lượng đã từng yêu cầu hoàn hàng
                var exited = existedRefundItems.FirstOrDefault(item => x.OrderItemId == item.OrderItemId)?.Quantity ?? 0;
                // Số lượng tối đa cho hoàn hàng
                var maxQuantity = order.OrderItems.FirstOrDefault(item => x.OrderItemId == item.OrderItemId)?.Quantity ?? 0;
                return maxQuantity < x.Quantity + exited;
            });
            // Nếu có thằng nào có tổng số lượng yêu cầu refund > số lượng đã đặt trong đơn hàng
            if (checkQuantity.Any(x => x))
                return (false, "Số lượng sản phẩm yêu cầu hoàn tiền vượt quá số lượng đã đặt trong đơn hàng!");
        }
        var refundRequest = new OrderRequestRefund
        {
            OrderId = order.OrderId,
            Images = request.Images,
            Items = orderItems.Select(x => new OrderRefundItem
            {
                OrderItemId = x.OrderItemId,
                Quantity = x.Quantity ?? 0
            }).ToList(),
            Reason = request.Reason,
            Status = OrderStatusData.Pending,
            UserModified = currentUserId,
            RequestDate = System.DateTime.Today            
        };
        order.Status = OrderStatusData.RequestRefund;
        await _unitOfWork.OrderRequestRefund.CreateAsync(refundRequest);
        await _unitOfWork.SaveAsync();
        refundRequest.Order = null;
        refundRequest.Items = refundRequest.Items.Select(x => { x.OrderRefund = null; return x; }).ToList();
        return (true, "Yêu cầu hoàn tiền đã được gửi thành công!");
    }

    public async Task<IBusinessResult> GetRefundAsync(int orderId)
    {
        var order = await _unitOfWork.Order.DbSet().Include(x => x.Refunds).AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (order == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Không tìm thấy thông tin hóa đơn!");
        var res = new RefundResponse
        {
            OrderId = orderId,
            RefundId = order.Refunds.Select(c => c.RequestRefundId)
        };
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách yêu cầu hoàn tiền thành công!", res);
    }
    
    public async Task<IBusinessResult> GetRefundDetailAsync(int refundId)
    {
        var refund = await _unitOfWork.OrderRequestRefund.DbSet().Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.RequestRefundId == refundId);
        if (refund == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Không tìm thấy yêu cầu hoàn tiền!", null);
        
        refund.Items = null;
        var orderItems = (await _unitOfWork.Order.DbSet().AsNoTracking()
                .Include(x => x.OrderItems).ThenInclude(x => x.Combo)
                .Include(x => x.OrderItems).ThenInclude(x => x.Accessory).ThenInclude(x => x.AccessoryImages)
                .Include(x => x.OrderItems).ThenInclude(x => x.TerrariumVariant)
                .FirstOrDefaultAsync(x => x.OrderId == refund.OrderId))
                ?.OrderItems;
        refund.Items = refund.Items?.Select(item => {
            item.OrderRefund = null;

            var orderItem = orderItems?.FirstOrDefault(oi => oi.OrderItemId == item.OrderItemId);
            if (orderItem != null)
            {
                if (orderItem.TerrariumVariant != null)
                {
                    item.Name = orderItem.TerrariumVariant.VariantName;
                    if (!string.IsNullOrEmpty(orderItem.TerrariumVariant.UrlImage))
                        item.Images = new[] { orderItem.TerrariumVariant.UrlImage };
                }
                else if (orderItem.Combo != null)
                {
                    item.Name = orderItem.Combo.Name;
                    if (!string.IsNullOrEmpty(orderItem.Combo.ImageUrl))
                        item.Images = new[] { orderItem.Combo.ImageUrl };
                }
                else if (orderItem.Accessory != null)
                {
                    item.Name = orderItem.Accessory.Name;
                    item.Images = orderItem.Accessory.AccessoryImages.Where(x => !string.IsNullOrEmpty(x.ImageUrl)).Select(x => x.ImageUrl);
                }
                else
                {
                    item.Name = "Underfine";
                    item.Images = Array.Empty<string>();
                }
            }

            return item;
        }).ToList();
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy thông tin yêu cầu hoàn tiền thành công!", refund);
    }

    public async Task<(bool, string, object?)> UpdateRequestRefundAsync(UpdateRefundRequest request, int currentUserId)
    {
        var refund = await _unitOfWork.OrderRequestRefund.DbSet().Include(x => x.Order).Include(x => x.Items).FirstOrDefaultAsync(x => x.RequestRefundId == request.RefundId);
        if (refund == null)
            return (false, "Không tìm thấy yêu cầu hoàn tiền!", null);
        if (refund.Order.UserId != currentUserId && request.Status != CommonData.OrderStatusData.Rejected)
            return (false, "Bạn không có quyền thực hiện thao tác này!", null);

        if (request.Status == CommonData.OrderStatusData.Pending)
            return (false, "Trạng thái cho đơn yêu cầu hoàn tiền không hợp lệ!", null);

        if ((request.Status == CommonData.OrderStatusData.Cancel || request.Status == CommonData.OrderStatusData.Approved || request.Status == CommonData.OrderStatusData.Rejected) && refund.Status != CommonData.OrderStatusData.Pending)
            return (false, "Yêu cầu hoàn tiền này không thể hủy!", null);

        if (request.Status == CommonData.OrderStatusData.Rejected && string.IsNullOrEmpty(request.Reason))
            return (false, "Lý do từ chối yêu cầu hoàn tiền không được để trống!", null);
        if (request.Status == CommonData.OrderStatusData.Approved && (!request.RefundAmount.HasValue && request.RefundAmount < 0))
            return (false, "Số tiền hoàn lại hoặc số điểm hoàn lại phải lớn hơn 0!", null);
        var order = await _unitOfWork.Order.FindOneAsync(x => x.OrderId == refund.OrderId);
        if (order == null)
            return (false, "Không tìm thấy đơn hàng cần hoàn tiền!", null);
        OrderTransport? transport = null;
        if (request.Status == CommonData.OrderStatusData.Approved && request.CreateTransport)
        {
            if (!request.EstimateCompletedDate.HasValue)
                return (false, "Vui lòng chọn ngày dự kiến sẽ lấy hàng hoàn tiền!", null);

            var users = await _unitOfWork.User.DbSet().AsNoTracking()
                .Where(x => x.UserId == request.UserId || x.UserId == currentUserId)
                .Select(x => new { x.UserId, x.Username }).ToArrayAsync();
            var assignUser = users.FirstOrDefault(x => x.UserId == request.UserId);
            var currentUser = users.FirstOrDefault(x => x.UserId == currentUserId);
            if ((request.UserId.HasValue && assignUser == null) || currentUser == null)
                return (false, "Thông tin người sẽ đến nhận hàng không hợp lệ!", null);

            transport = new OrderTransport
            {
                OrderId = refund.OrderId,
                Status = TransportStatusEnum.InCustomer,
                EstimateCompletedDate = request.EstimateCompletedDate.Value,
                Note = request.Note,
                IsRefund = true,
                UserId = assignUser?.UserId,
                CreatedDate = System.DateTime.Now,
                CreatedBy = currentUser.Username,
                Items = refund.Items.Where(x => x.IsApproved == true).Select(x => new OrderTransportItem
                {
                    OrderItemId = x.OrderItemId,
                    Quantity = x.Quantity
                }).ToList()
            };
        }
        using (var trans = await _unitOfWork.OrderRequestRefund.BeginTransactionAsync())
        {
            refund.Status = request.Status;
            refund.ReasonModified = request.Reason;
            refund.UserModified = currentUserId;
            refund.LastModifiedDate = System.DateTime.Now;
            refund.IsPoint = request.IsPoint;
            if (refund.Status == CommonData.OrderStatusData.Approved)
            {
                order.Status = OrderStatusData.Refuning;
                refund.RefundAmount = request.RefundAmount;
            }
            else
            {
                order.Status = OrderStatusData.Completed;
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
                return (false, "Cập nhật yêu cầu hoàn tiền thất bại: " + ex.Message, null);
            }
            finally
            {
                if (isRollback)
                    await trans.RollbackAsync();
                else
                    await trans.CommitAsync();
            }
        }    
        refund.Order = null;
        refund.Items = refund.Items.Select(x => { x.OrderRefund = null; return x; }).ToList();
        return (true, "Cập nhật yêu cầu hoàn tiền thành công!", refund);
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
                        TerrariumId = item.TerrariumId,
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
                        TerrariumId = item.TerrariumId,
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
                TerrariumId = i.TerrariumId,
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