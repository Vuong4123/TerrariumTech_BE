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
                VoucherId = order.VoucherId,
                TotalAmount = order.TotalAmount,
                OriginalAmount = order.OriginalAmount, // ✅ THÊM
                DiscountAmount = order.DiscountAmount,   // ✅ THÊM
                Deposit = order.Deposit,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus ?? string.Empty,
                TransactionId = order.TransactionId,
                Note = order.Note,
                IsPayFull = order.IsPayFull,
                Refunds = new List<RefundResponseOrder>(order.Refunds?.Select(r => new RefundResponseOrder
                {
                    Status = r.Status,
                    Reason = r.Reason
                }) ?? Enumerable.Empty<RefundResponseOrder>()).ToList(),
                OrderItems = new List<OrderItemResponse>()
            };

            // ✅ CHỈ LẤY MAIN ITEMS (không có parent)
            var mainItems = order.OrderItems.Where(x => x.ParentOrderItemId == null).ToList();

            foreach (var item in mainItems)
            {
                var itemResponse = await BuildOrderItemResponseAsync(item, order.OrderItems);
                orderResponse.OrderItems.Add(itemResponse);
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
            VoucherId = order.VoucherId,
            AddressId = order.AddressId,
            TotalAmount = order.TotalAmount,
            OriginalAmount = order.OriginalAmount, // ✅ THÊM
            DiscountAmount = order.DiscountAmount,   // ✅ THÊM
            Deposit = order.Deposit,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Note = order.Note,
            PaymentStatus = order.PaymentStatus ?? string.Empty,
            TransactionId = order.TransactionId,
            IsPayFull = order.IsPayFull,
            Refunds = new List<RefundResponseOrder>(order.Refunds?.Select(r => new RefundResponseOrder
            {
                Status = r.Status,
                Reason = r.Reason
            }) ?? Enumerable.Empty<RefundResponseOrder>()).ToList(),
            OrderItems = new List<OrderItemResponse>()
        };

        // ✅ CHỈ LẤY MAIN ITEMS
        var mainItems = order.OrderItems.Where(x => x.ParentOrderItemId == null).ToList();

        foreach (var item in mainItems)
        {
            var itemResponse = await BuildOrderItemResponseAsync(item, order.OrderItems);
            orderResponse.OrderItems.Add(itemResponse);
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
                OriginalAmount = order.OriginalAmount, // ✅ THÊM
                DiscountAmount = order.DiscountAmount,   // ✅ THÊM
                Deposit = order.Deposit,
                OrderDate = order.OrderDate,
                Note = order.Note,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus ?? string.Empty,
                TransactionId = order.TransactionId,
                IsPayFull = order.IsPayFull,
                Refunds = new List<RefundResponseOrder>(order.Refunds?.Select(r => new RefundResponseOrder
                {
                    Status = r.Status,
                    Reason = r.Reason
                }) ?? Enumerable.Empty<RefundResponseOrder>()).ToList(),
                OrderItems = new List<OrderItemResponse>()
            };

            //// ✅ CHỈ LẤY MAIN ITEMS
            //var mainItems = order.OrderItems.Where(x => x.ParentOrderItemId == null).ToList();

            //foreach (var item in mainItems)
            //{
            //    var itemResponse = await BuildOrderItemResponseAsync(item, order.OrderItems);
            //    orderResponse.OrderItems.Add(itemResponse);
            //}

            result.Add(orderResponse);
        }

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách đơn hàng thành công", result);
    }
    private async Task<OrderItemResponse> BuildOrderItemResponseAsync(OrderItem item, ICollection<OrderItem> allOrderItems)
    {
        var response = new OrderItemResponse
        {
            OrderItemId = item.OrderItemId,
            ComboId = item.ComboId ?? 0,
            ItemType = item.ItemType,
            TerrariumId = item.TerrariumId,
            AccessoryId = item.AccessoryId,
            TerrariumVariantId = item.TerrariumVariantId,
            AccessoryQuantity = item.AccessoryQuantity,
            TerrariumVariantQuantity = item.TerrariumVariantQuantity,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice,
            ParentOrderItemId = item.ParentOrderItemId,
            ChildItems = new List<OrderItemResponse>()
        };

        // ✅ SET PRODUCT NAME & IMAGE
        await SetProductDetailsAsync(response, item);

        // ✅ NẾU LÀ COMBO, LẤY CHILD ITEMS
        if (item.ItemType == "Combo" && item.ComboId.HasValue)
        {
            var childItems = allOrderItems
                .Where(x => x.ParentOrderItemId == item.OrderItemId)
                .ToList();

            foreach (var childItem in childItems)
            {
                var childResponse = new OrderItemResponse
                {
                    OrderItemId = childItem.OrderItemId,
                    ComboId = childItem.ComboId ?? 0,
                    ItemType = childItem.ItemType,
                    TerrariumId = childItem.TerrariumId,
                    IsFeedBack = childItem.Feedbacks != null && childItem.Feedbacks.Any(),
                    AccessoryId = childItem.AccessoryId,
                    TerrariumVariantId = childItem.TerrariumVariantId,
                    AccessoryQuantity = childItem.AccessoryQuantity,
                    TerrariumVariantQuantity = childItem.TerrariumVariantQuantity,
                    Quantity = childItem.Quantity,
                    UnitPrice = childItem.UnitPrice,
                    TotalPrice = childItem.TotalPrice,
                    ParentOrderItemId = childItem.ParentOrderItemId
                };

                // Set product details cho child item
                await SetProductDetailsAsync(childResponse, childItem);

                response.ChildItems.Add(childResponse);
            }
        }

        return response;
    }

    private async Task SetProductDetailsAsync(OrderItemResponse response, OrderItem item)
    {
        try
        {
            if (item.ComboId.HasValue && item.ItemType == "Combo")
            {
                var combo = await _unitOfWork.Combo.GetByIdAsync(item.ComboId.Value);
                response.ProductName = combo?.Name ?? "Unknown Combo";
                response.ImageUrl = combo?.ImageUrl;
            }
            else if (item.AccessoryId.HasValue)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                response.ProductName = accessory?.Name ?? "Unknown Accessory";

                var images = await _unitOfWork.AccessoryImage.GetAllByAccessoryIdAsync(item.AccessoryId.Value);
                response.ImageUrl = images?.FirstOrDefault()?.ImageUrl;
            }
            else if (item.TerrariumVariantId.HasValue)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                response.ProductName = variant?.VariantName ?? "Unknown Variant";
                response.ImageUrl = variant?.UrlImage;

                if (item.TerrariumId.HasValue)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(item.TerrariumId.Value);
                    response.ProductName = $"{terrarium?.TerrariumName} - {variant?.VariantName}";
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error setting product details for OrderItem {item.OrderItemId}");
            response.ProductName = "Unknown Product";
        }
    }

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
            OrderStatusData.Pending
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
    public async Task<IBusinessResult> RejectOrderAsync(int orderId, int staffId, RejectOrderRequest request)
    {
        using var transaction = await _unitOfWork.Order.BeginTransactionAsync();

        try
        {
            // 1. Lấy thông tin đơn hàng
            var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
            if (order == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy đơn hàng");

            // 2. Validate trạng thái có thể reject
            var rejectableStatuses = new[] {
            OrderStatusData.Pending,
            OrderStatusData.Processing
        };

            if (!rejectableStatuses.Contains(order.Status))
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE,
                    $"Không thể từ chối đơn hàng ở trạng thái {order.Status}");
            }

            // 3. Validate lý do từ chối
            if (string.IsNullOrWhiteSpace(request.RejectReason))
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Vui lòng nhập lý do từ chối");

            // 4. Cập nhật trạng thái đơn hàng
            order.Status = OrderStatusData.Rejected;
            // 6. Xử lý hoàn tiền vào ví (nếu đã thanh toán)
            decimal refundAmount = 0;
            if (order.PaymentStatus == "Paid" || order.Deposit > 0)
            {
                refundAmount = await ProcessRefundForRejectedOrderAsync(order, request.RejectReason);
            }

            // 7. Hoàn lại stock cho sản phẩm
            await RestoreStockForOrderItemsAsync(orderId);

            // 8. Hoàn lại voucher (nếu có)
            if (order.VoucherId.HasValue)
            {
                await RestoreVoucherUsageAsync(order.VoucherId.Value, order.UserId);
            }

            // 10. Save changes
            await _unitOfWork.Order.UpdateAsync(order);
            await _unitOfWork.SaveAsync();
            await transaction.CommitAsync();

            var response = new RejectOrderResponse
            {
                OrderId = orderId,
                Status = order.Status,
                RejectedAt = DateTime.UtcNow,
                RejectReason = request.RejectReason,
                RejectedBy = staffId.ToString(),
                RefundAmount = refundAmount,
                RefundStatus = refundAmount > 0 ? "Đã hoàn tiền vào ví" : "Không có tiền cần hoàn",
                Message = "Từ chối đơn hàng thành công"
            };

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Từ chối đơn hàng thành công", response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error rejecting order {OrderId} by staff {StaffId}", orderId, staffId);
            return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi từ chối đơn hàng: {ex.Message}");
        }
    }

    // Helper method để hoàn tiền cho rejected order
    private async Task<decimal> ProcessRefundForRejectedOrderAsync(Order order, string rejectReason)
    {
        decimal refundAmount = 0;

        // Tính toán số tiền hoàn trả
        if (order.PaymentStatus == "Paid")
        {
            refundAmount = order.TotalAmount;
        }
        else if (order.Deposit > 0)
        {
            refundAmount = order.Deposit.Value;
        }

        if (refundAmount > 0)
        {
            // Lấy ví của user
            var userWallet = await _unitOfWork.Wallet.FindOneAsync(w =>
                w.UserId == order.UserId && w.WalletType == "User");

            if (userWallet != null)
            {
                // Cộng tiền vào ví user
                userWallet.Balance += refundAmount;
                await _unitOfWork.Wallet.UpdateAsync(userWallet);

                // Trừ tiền từ ví processing revenue
                var processingWallet = await _unitOfWork.Wallet.FindOneAsync(w =>
                    w.WalletType == "ProcessingRevenue");
                if (processingWallet != null && processingWallet.Balance >= refundAmount)
                {
                    processingWallet.Balance -= refundAmount;
                    await _unitOfWork.Wallet.UpdateAsync(processingWallet);
                }

                // Tạo transaction history
                await _unitOfWork.WalletTransactionRepository.CreateAsync(new WalletTransaction
                {
                    WalletId = userWallet.WalletId,
                    Amount = refundAmount,
                    Type = "Refund",
                    CreatedDate = DateTime.UtcNow,
                    OrderId = order.OrderId
                });

                // Cập nhật payment status
                order.PaymentStatus = OrderStatusData.Refunded;
            }
        }

        return refundAmount;
    }

    // Helper method để restore voucher usage
    private async Task RestoreVoucherUsageAsync(int voucherId, int userId)
    {
        try
        {
            var voucher = await _unitOfWork.Voucher.GetByIdAsync(voucherId);
            if (voucher != null)
            {
                // Tăng lại remaining usage
                voucher.RemainingUsage += 1;
                await _unitOfWork.Voucher.UpdateAsync(voucher);

                // Giảm user usage count
                var voucherUsage = await _unitOfWork.VoucherUsageRepository
                    .FindOneAsync(vu => vu.VoucherId == voucherId && vu.UserId == userId.ToString());

                if (voucherUsage != null && voucherUsage.UsedCount > 0)
                {
                    voucherUsage.UsedCount -= 1;
                    await _unitOfWork.VoucherUsageRepository.UpdateAsync(voucherUsage);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring voucher {VoucherId} for user {UserId}", voucherId, userId);
        }
    }

    // Đổi tên từ ValidateAITerrariumStockAsync thành ValidateVariantAccessoryStockAsync
    private async Task<StockValidationResult> ValidateVariantAccessoryStockAsync(int terrariumVariantId, int quantity)
    {
        try
        {
            // Lấy danh sách accessories của variant này
            var variantAccessories = await _unitOfWork.TerrariumVariantAccessory
                .GetByTerrariumVariantIdAsync(terrariumVariantId);

            if (!variantAccessories.Any())
            {
                return new StockValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Terrarium variant chưa có accessories được định nghĩa"
                };
            }

            // Kiểm tra stock của từng accessory trong variant
            foreach (var item in variantAccessories)
            {
                foreach (var va in item.TerrariumVariantAccessories)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(va.AccessoryId);
                    if (accessory == null)
                    {
                        return new StockValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = $"Accessory ID {va.AccessoryId} không tồn tại"
                        };
                    }

                    // Tính số lượng accessory cần cho order
                    int requiredQty = va.Quantity * quantity;

                    // Kiểm tra stock
                    if (accessory.StockQuantity < requiredQty)
                    {
                        return new StockValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = $"Accessory '{accessory.Name}' trong variant không đủ hàng. " +
                                         $"Còn lại: {accessory.StockQuantity}, " +
                                         $"Cần: {requiredQty} (cho {quantity} terrarium variant)"
                        };
                    }
                }
            }

            return new StockValidationResult { IsValid = true };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error validating variant accessory stock for variant {VariantId}", terrariumVariantId);
            return new StockValidationResult
            {
                IsValid = false,
                ErrorMessage = "Lỗi khi kiểm tra tồn kho variant accessories"
            };
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
        var order = _unitOfWork.Order.GetOrderbyIdAsync(orderId);

        foreach (var item in order.Result.OrderItems)
        {
            // Hoàn stock cho accessory riêng lẻ (không thuộc variant)
            if (item.AccessoryId.HasValue && item.AccessoryQuantity > 0)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                if (accessory != null)
                {
                    accessory.StockQuantity += item.AccessoryQuantity.Value;
                    await _unitOfWork.Accessory.UpdateAsync(accessory);
                }
            }

            // ✅ THAY ĐỔI: Hoàn stock cho variant và accessories của variant
            if (item.TerrariumVariantId.HasValue && item.TerrariumVariantQuantity > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                if (variant != null)
                {
                    // Hoàn stock variant (nếu cần)
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                    if (terrarium != null && !terrarium.GeneratedByAI)
                    {
                        variant.StockQuantity += item.TerrariumVariantQuantity.Value;
                        await _unitOfWork.TerrariumVariant.UpdateAsync(variant);
                    }

                    // ✅ HOÀN STOCK CHO ACCESSORIES CỦA VARIANT
                    await RestoreVariantAccessoryStockAsync(variant.TerrariumVariantId, item.TerrariumVariantQuantity.Value);
                }
            }
        }
    }

    // ✅ THÊM METHOD MỚI
    private async Task RestoreVariantAccessoryStockAsync(int terrariumVariantId, int variantQuantity)
    {
        var variantAccessories = await _unitOfWork.TerrariumVariantAccessory
            .GetByTerrariumVariantIdAsync(terrariumVariantId);

        foreach (var vaItem in variantAccessories)
        {
            foreach (var va in vaItem.TerrariumVariantAccessories)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(va.AccessoryId);
                if (accessory != null)
                {
                    int accessoryQtyToRestore = va.Quantity * variantQuantity;
                    accessory.StockQuantity += accessoryQtyToRestore;
                    await _unitOfWork.Accessory.UpdateAsync(accessory);
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
            var order = await _unitOfWork.Order.GetOrderWithItemsAsync(refundRequest.OrderId);
            if (order == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy đơn hàng liên quan");

            try
            {
                // 4. Xử lý approve/reject
                if (request.IsApproved)
                {
                    // APPROVE - Thực hiện hoàn tiền
                    refundRequest.Status = OrderStatusData.Approved;
                    refundRequest.UserModified = staffId;
                    refundRequest.LastModifiedDate = DateTime.UtcNow;

                    // ✅ HOÀN LẠI STOCK KHI APPROVE REFUND
                    await RestoreStockForRefundedOrder(order);

                    // Thực hiện hoàn tiền vào ví
                    var refundResult = await ProcessActualRefundAsync(order, refundRequest);
                    if (!refundResult.Success)
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE,
                            $"Lỗi khi hoàn tiền: {refundResult.Message}");
                    }

                        order.Status = OrderStatusData.Refunded;

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
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting refund request {RefundId}", refundId);
            return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi xử lý yêu cầu hoàn tiền: {ex.Message}");
        }
    }

    // ✅ HOÀN LẠI STOCK KHI REFUND
    private async Task RestoreStockForRefundedOrder(Order order)
    {
        if (order.OrderItems == null || !order.OrderItems.Any())
            return;

        foreach (var item in order.OrderItems)
        {
            // ✅ 1. HOÀN LẠI STOCK ACCESSORY RIÊNG LẺ
            if (item.AccessoryId.HasValue && item.AccessoryQuantity > 0)
            {
                await _unitOfWork.Accessory.RestoreStockAsync(item.AccessoryId.Value, item.AccessoryQuantity ?? 0);
            }

            // ✅ 2. HOÀN LẠI CHO AI TERRARIUM
            if (item.TerrariumVariantId.HasValue && item.TerrariumVariantQuantity > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);

                        await RestoreStockForAITerrarium(terrarium.TerrariumId, item.TerrariumVariantQuantity ?? 0);
                    // Non-AI terrarium: Không cần hoàn vì không bị trừ
                }
            }

            // ✅ 3. HOÀN LẠI CHO COMBO
            if (item.ComboId.HasValue)
            {
                await RestoreStockForComboItem(item);
            }
        }
    }

    // ✅ HOÀN LẠI STOCK CHO AI TERRARIUM
    private async Task RestoreStockForAITerrarium(int terrariumId, int terrariumQuantity)
    {
        var terrariumAccessories = await _unitOfWork.TerrariumAccessory.GetByTerrariumIdAsync(terrariumId);

        foreach (var ta in terrariumAccessories)
        {
            // Hoàn lại stock accessories
            await _unitOfWork.Accessory.RestoreStockAsync(ta.AccessoryId, terrariumQuantity);
        }
    }

    // ✅ HOÀN LẠI STOCK CHO COMBO
    private async Task RestoreStockForComboItem(OrderItem comboOrderItem)
    {
        if (!comboOrderItem.ComboId.HasValue) return;

        var combo = await _unitOfWork.Combo.GetByIdAsync(comboOrderItem.ComboId.Value);
        if (combo?.ComboItems == null) return;

        foreach (var comboItem in combo.ComboItems)
        {
            // Hoàn lại accessory trong combo
            if (comboItem.AccessoryId.HasValue)
            {
                await _unitOfWork.Accessory.RestoreStockAsync(comboItem.AccessoryId.Value, 1);
            }

            // Hoàn lại terrarium trong combo
            if (comboItem.TerrariumVariantId.HasValue)
            {
                await _unitOfWork.TerrariumVariant.RestoreStockAsync(comboItem.TerrariumVariantId.Value, 1);
            }
        }
    }
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

    public async Task<int> CreateAsync(OrderCreateRequest request)
    {
        ValidateCreateRequest(request);
        if (request.VoucherId == 0) request.VoucherId = null;

        int userId = _userContextService.GetCurrentUser();
        var orderItems = new List<OrderItem>();

        int? safeVoucherId = null;
        decimal discountAmount = 0m;
        decimal originalAmount = request.TotalAmountOld;
        decimal finalAmount = request.TotalAmountNew;

        // ========= VALIDATE VOUCHER =========
        if (request.VoucherId.HasValue && request.VoucherId.Value > 0)
        {
            var voucher = await _unitOfWork.Voucher.GetByIdAsync(request.VoucherId.Value);
            if (voucher != null)
            {
                bool isValid =
                    voucher.Status == VoucherStatus.Active.ToString() &&
                    voucher.ValidFrom <= DateTime.UtcNow &&
                    voucher.ValidTo >= DateTime.UtcNow;

                if (isValid && voucher.MinOrderAmount.HasValue && originalAmount < voucher.MinOrderAmount.Value)
                {
                    throw new ArgumentException($"Đơn hàng tối thiểu {voucher.MinOrderAmount.Value:N0}đ để sử dụng voucher này.");
                }

                if (isValid && voucher.IsPersonal)
                {
                    throw new ArgumentException("Voucher cá nhân, không thuộc về bạn.");
                }

                if (isValid)
                {
                    safeVoucherId = voucher.VoucherId;
                    discountAmount = voucher.DiscountAmount ?? 0;
                }
            }
        }

        // ========= XỬ LÝ TẤT CẢ ITEMS (BAO GỒM NHIỀU COMBO) =========
        foreach (var reqItem in request.Items)
        {
            // ✅ 1. XỬ LÝ COMBO
            if (reqItem.ComboId.HasValue && (reqItem.ComboQuantity ?? 0) > 0)
            {
                await ProcessComboItem(reqItem, orderItems);
            }
            // ✅ 2. XỬ LÝ ACCESSORY RIÊNG LẺ
            else if (reqItem.AccessoryId.HasValue && (reqItem.AccessoryQuantity ?? 0) > 0)
            {
                await ProcessAccessoryItem(reqItem, orderItems);
            }
            // ✅ 3. XỬ LÝ TERRARIUM VARIANT
            else if (reqItem.TerrariumVariantId.HasValue && (reqItem.TerrariumVariantQuantity ?? 0) > 0)
            {
                await ProcessTerrariumVariantItem(reqItem, orderItems);
            }
        }

        var order = new Order
        {
            UserId = userId,
            VoucherId = safeVoucherId,
            AddressId = request.AddressId,
            Deposit = request.Deposit,
            OriginalAmount = originalAmount,
            TotalAmount = finalAmount,
            DiscountAmount = discountAmount,
            Note = request.Note,
            IsPayFull = request.IsPayFull,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatusData.Pending,
            PaymentStatus = "Unpaid",
            OrderItems = orderItems,
        };

        try
        {
            await _unitOfWork.Order.CreateAsync(order);
            await DeductStockAsync(orderItems);
            await _unitOfWork.SaveAsync();
            // ✅ CONSUME VOUCHER SAU KHI TẠO ORDER THÀNH CÔNG
            if (safeVoucherId.HasValue)
            {
                await ConsumeVoucher(safeVoucherId.Value, userId.ToString(), originalAmount);
            }

            return order.OrderId;
        }
        catch (Exception ex)
        {
            var msg = ex.Message + (ex.InnerException != null ? " | INNER: " + ex.InnerException.Message : "");
            throw new Exception("Order create error: " + msg, ex);
        }
    }

    private async Task DeductStockAsync(List<OrderItem> orderItems)
    {
        // 1. ✅ TRỪ STOCK ACCESSORIES (chỉ lấy accessories riêng lẻ, không trong combo)
        var accessoryGroups = orderItems
            .Where(x => x.AccessoryId.HasValue &&
                        x.ItemType != CommonData.CartItemType.COMBO) // ✅ Loại trừ items trong combo
            .GroupBy(x => x.AccessoryId.Value)
            .Select(g => new {
                AccessoryId = g.Key,
                TotalQuantity = g.Sum(x => x.AccessoryQuantity ?? x.Quantity) // ✅ Ưu tiên AccessoryQuantity
            });

        foreach (var group in accessoryGroups)
        {
            var accessory = await _unitOfWork.Accessory.GetByIdAsync(group.AccessoryId);
            if (accessory != null)
            {
                accessory.StockQuantity -= group.TotalQuantity;
                if (accessory.StockQuantity < 0)
                {
                    throw new InvalidOperationException($"Stock không đủ cho accessory {accessory.Name}");
                }
                await _unitOfWork.Accessory.UpdateAsync(accessory);
            }
        }

        // 2. ✅ TRỪ STOCK TERRARIUM VARIANTS (chỉ lấy variants riêng lẻ, không trong combo)
        var variantGroups = orderItems
            .Where(x => x.TerrariumVariantId.HasValue &&
                        x.ItemType != CommonData.CartItemType.COMBO) // ✅ Loại trừ items trong combo
            .GroupBy(x => x.TerrariumVariantId.Value)
            .Select(g => new {
                VariantId = g.Key,
                TotalQuantity = g.Sum(x => x.TerrariumVariantQuantity ?? x.Quantity) // ✅ Ưu tiên TerrariumVariantQuantity
            });

        foreach (var group in variantGroups)
        {
            var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(group.VariantId);
            if (variant != null)
            {
                var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);

                // Chỉ trừ stock cho non-AI terrariums
                if (terrarium != null && !terrarium.GeneratedByAI)
                {
                    variant.StockQuantity -= (int)group.TotalQuantity;
                    if (variant.StockQuantity < 0)
                    {
                        throw new InvalidOperationException($"Stock không đủ cho terrarium variant {variant.TerrariumVariantId}");
                    }
                    await _unitOfWork.TerrariumVariant.UpdateAsync(variant);
                }
                // Nếu là AI terrarium, trừ stock của các accessories trong variant
                else if (terrarium?.GeneratedByAI == true)
                {
                    await DeductVariantAccessoryStockAsync(variant.TerrariumVariantId, (int)group.TotalQuantity);
                }
            }
        }

        // 3. ✅ TRỪ STOCK COMBOS VÀ ITEMS BÊN TRONG
        var comboGroups = orderItems
            .Where(x => x.ComboId.HasValue &&
                        x.ItemType == CommonData.CartItemType.COMBO &&
                        x.UnitPrice > 0) // Chỉ lấy main combo items
            .GroupBy(x => x.ComboId.Value)
            .Select(g => new {
                ComboId = g.Key,
                TotalQuantity = g.Sum(x => x.Quantity)
            });

        foreach (var group in comboGroups)
        {
            var combo = await _unitOfWork.Combo.GetByIdAsync(group.ComboId);
            if (combo != null)
            {
                // Trừ stock của combo
                combo.StockQuantity -= (int)group.TotalQuantity;
                if (combo.StockQuantity < 0)
                {
                    throw new InvalidOperationException($"Stock không đủ cho combo {combo.Name}");
                }
                await _unitOfWork.Combo.UpdateAsync(combo);

                // ✅ TRỪ STOCK CỦA ITEMS TRONG COMBO
                await DeductComboItemsStockAsync(combo, (int)group.TotalQuantity);
            }
        }
    }

    // ✅ METHOD MỚI: TRỪ STOCK CHO ITEMS TRONG COMBO
    private async Task DeductComboItemsStockAsync(Combo combo, int comboQuantity)
    {
        if (combo.ComboItems == null) return;

        foreach (var comboItem in combo.ComboItems)
        {
            // Trừ stock accessories trong combo
            if (comboItem.AccessoryId.HasValue)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(comboItem.AccessoryId.Value);
                if (accessory != null)
                {
                    var requiredQty = comboItem.Quantity * comboQuantity;
                    accessory.StockQuantity -= requiredQty;

                    if (accessory.StockQuantity < 0)
                    {
                        throw new InvalidOperationException($"Stock không đủ cho accessory {accessory.Name} trong combo");
                    }

                    await _unitOfWork.Accessory.UpdateAsync(accessory);
                }
            }

            // Trừ stock terrarium variants trong combo
            if (comboItem.TerrariumVariantId.HasValue)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(comboItem.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);

                    if (terrarium != null && !terrarium.GeneratedByAI)
                    {
                        var requiredQty = comboItem.Quantity * comboQuantity;
                        variant.StockQuantity -= requiredQty;

                        if (variant.StockQuantity < 0)
                        {
                            throw new InvalidOperationException($"Stock không đủ cho terrarium variant trong combo");
                        }

                        await _unitOfWork.TerrariumVariant.UpdateAsync(variant);
                    }
                    else if (terrarium?.GeneratedByAI == true)
                    {
                        var requiredQty = comboItem.Quantity * comboQuantity;
                        await DeductVariantAccessoryStockAsync(variant.TerrariumVariantId, requiredQty);
                    }
                }
            }
        }
    }


    // ✅ METHOD TRỪ STOCK CHO AI TERRARIUM ACCESSORIES
    private async Task DeductVariantAccessoryStockAsync(int variantId, int quantity)
    {
        // Sử dụng method có sẵn
        var variantAccessories = await _unitOfWork.TerrariumVariantAccessory
            .GetByTerrariumVariantId(variantId);

        foreach (var va in variantAccessories)
        {
            if (va.Accessory != null)
            {
                var requiredQty = va.Quantity * quantity;
                va.Accessory.StockQuantity -= requiredQty;

                if (va.Accessory.StockQuantity < 0)
                {
                    throw new InvalidOperationException($"Stock không đủ cho accessory {va.Accessory.Name} trong AI terrarium");
                }

                await _unitOfWork.Accessory.UpdateAsync(va.Accessory);
            }
        }
    }

    // ✅ XỬ LÝ COMBO ITEM
    private async Task ProcessComboItem(OrderItemCreateRequest reqItem, List<OrderItem> orderItems)
    {
        var combo = await _unitOfWork.Combo.GetByIdAsync(reqItem.ComboId.Value);
        if (combo == null)
            throw new ArgumentException($"Combo {reqItem.ComboId} không tồn tại");

        // Validate combo stock
        await ValidateComboStock(combo, reqItem.ComboQuantity ?? 1);

        // ✅ TẠO ORDER ITEM CHÍNH CHO COMBO
        var comboMainItem = new OrderItem
        {
            ComboId = reqItem.ComboId,
            AccessoryId = null,
            TerrariumId = null,
            TerrariumVariantId = null,
            AccessoryQuantity = 0,
            TerrariumVariantQuantity = 0,
            Quantity = reqItem.ComboQuantity ?? 1,
            UnitPrice = combo.ComboPrice,
            ItemType = CommonData.CartItemType.COMBO,
            TotalPrice = combo.ComboPrice * (reqItem.ComboQuantity ?? 1),
            ParentOrderItemId = null
        };

        orderItems.Add(comboMainItem);

        // ✅ TẠO ORDER ITEMS CHI TIẾT CHO TỪNG ITEM TRONG COMBO
        foreach (var comboItem in combo.ComboItems)
        {
            for (int i = 0; i < (reqItem.ComboQuantity ?? 1); i++)
            {
                if (comboItem.AccessoryId.HasValue)
                {
                    orderItems.Add(new OrderItem
                    {
                        ComboId = reqItem.ComboId,
                        AccessoryId = comboItem.AccessoryId,
                        TerrariumId = comboItem.TerrariumId,
                        TerrariumVariantId = null,
                        AccessoryQuantity = comboItem.Quantity,
                        TerrariumVariantQuantity = 0,
                        Quantity = comboItem.Quantity,
                        UnitPrice = 0,
                        ItemType = CommonData.CartItemType.COMBO,
                        TotalPrice = 0,
                        ParentOrderItemId = null
                    });
                }

                if (comboItem.TerrariumVariantId.HasValue)
                {
                    orderItems.Add(new OrderItem
                    {
                        ComboId = reqItem.ComboId,
                        AccessoryId = null,
                        TerrariumId = comboItem.TerrariumId,
                        TerrariumVariantId = comboItem.TerrariumVariantId,
                        AccessoryQuantity = 0,
                        TerrariumVariantQuantity = comboItem.Quantity,
                        Quantity = comboItem.Quantity,
                        UnitPrice = 0,
                        ItemType = CommonData.CartItemType.COMBO,
                        TotalPrice = 0,
                        ParentOrderItemId = null
                    });
                }
            }
        }
    }

    // ✅ XỬ LÝ ACCESSORY ITEM
    private async Task ProcessAccessoryItem(OrderItemCreateRequest reqItem, List<OrderItem> orderItems)
    {
        var acc = await _unitOfWork.Accessory.GetByIdAsync(reqItem.AccessoryId.Value);
        if (acc == null)
            throw new ArgumentException($"Accessory {reqItem.AccessoryId} không tồn tại");

        int qty = reqItem.AccessoryQuantity ?? 0;

        if (acc.StockQuantity < qty)
            throw new ArgumentException($"Accessory '{acc.Name}' không đủ hàng. Còn lại: {acc.StockQuantity}, yêu cầu: {qty}");

        var terrariumVariant = _unitOfWork.TerrariumVariantAccessory.GetByAccessoryId(reqItem.AccessoryId ?? 0);
        decimal unit = acc.Price;
        decimal line = unit * qty;

        orderItems.Add(new OrderItem
        {
            AccessoryId = reqItem.AccessoryId,
            TerrariumId = reqItem.TerrariumId,
            TerrariumVariantId = terrariumVariant.Result.TerrariumVariantId,
            AccessoryQuantity = qty,
            TerrariumVariantQuantity = 0,
            Quantity = qty,
            UnitPrice = unit,
            ItemType = reqItem.ItemType,
            TotalPrice = line,
            ParentOrderItemId = null
        });
    }

    // ✅ XỬ LÝ TERRARIUM VARIANT ITEM
    private async Task ProcessTerrariumVariantItem(OrderItemCreateRequest reqItem, List<OrderItem> orderItems)
    {
        var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(reqItem.TerrariumVariantId.Value);
        if (variant == null)
            throw new ArgumentException($"TerrariumVariant {reqItem.TerrariumVariantId} không tồn tại");

        var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
        if (terrarium == null)
            throw new ArgumentException($"Terrarium {variant.TerrariumId} không tồn tại");

        int qty = reqItem.TerrariumVariantQuantity ?? 0;

        // CHECK STOCK CHỈ CHO NON-AI TERRARIUM
        if (!terrarium.GeneratedByAI && variant.StockQuantity < qty)
            throw new ArgumentException($"Terrarium '{terrarium.TerrariumName}' không đủ hàng. Còn lại: {variant.StockQuantity}, yêu cầu: {qty}");

        // CHECK ACCESSORIES CHO AI TERRARIUM
        if (terrarium.GeneratedByAI)
        {
            var stockValidation = await ValidateVariantAccessoryStockAsync(variant.TerrariumVariantId, qty);
            if (!stockValidation.IsValid)
                throw new ArgumentException(stockValidation.ErrorMessage);
        }

        decimal unit = variant.Price;
        decimal line = unit * qty;

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
            TotalPrice = line,
            ParentOrderItemId = null
        });
    }

    // ✅ VALIDATE COMBO STOCK WITH QUANTITY
    private async Task ValidateComboStock(Combo combo, int quantity)
    {
        if (combo.StockQuantity < quantity)
            throw new ArgumentException($"Combo '{combo.Name}' không đủ hàng. Còn lại: {combo.StockQuantity}, yêu cầu: {quantity}");

        // Validate stock của items trong combo
        if (combo.ComboItems != null)
        {
            foreach (var comboItem in combo.ComboItems)
            {
                if (comboItem.AccessoryId.HasValue)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(comboItem.AccessoryId.Value);
                    if (accessory != null)
                    {
                        int requiredQty = comboItem.Quantity * quantity;
                        if (accessory.StockQuantity < requiredQty)
                            throw new ArgumentException($"Accessory '{accessory.Name}' trong combo không đủ hàng. Còn: {accessory.StockQuantity}, cần: {requiredQty}");
                    }
                }

                if (comboItem.TerrariumVariantId.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(comboItem.TerrariumVariantId.Value);
                    if (variant != null)
                    {
                        var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                        if (terrarium?.GeneratedByAI == true)
                        {
                            int requiredVariantQty = comboItem.Quantity * quantity;
                            var stockValidation = await ValidateVariantAccessoryStockAsync(variant.TerrariumVariantId, requiredVariantQty);
                            if (!stockValidation.IsValid)
                                throw new ArgumentException(stockValidation.ErrorMessage);
                        }
                    }
                }
            }
        }
    }
    // ✅ VALIDATE STOCK AVAILABILITY (cập nhật để handle combo)
    private async Task ValidateStockAvailability(OrderCreateRequest request)
    {
        // Check accessories riêng lẻ
        foreach (var item in request.Items.Where(i => i.AccessoryId.HasValue))
        {
            var acc = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
            if (acc == null)
                throw new ArgumentException($"Accessory {item.AccessoryId} không tồn tại");

            int requiredQty = item.AccessoryQuantity ?? 0;
            if (acc.StockQuantity < requiredQty)
                throw new ArgumentException($"'{acc.Name}' không đủ hàng. Còn: {acc.StockQuantity}, cần: {requiredQty}");
        }

        // Check terrarium variants riêng lẻ
        foreach (var item in request.Items.Where(i => i.TerrariumVariantId.HasValue))
        {
            var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
            if (variant == null)
                throw new ArgumentException($"TerrariumVariant {item.TerrariumVariantId} không tồn tại");

            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
            if (terrarium == null)
                throw new ArgumentException($"Terrarium không tồn tại");

            // ✅ CHỈ CHECK STOCK NÕU KHÔNG PHẢI AI
            if (!terrarium.GeneratedByAI)
            {
                int requiredQty = item.TerrariumVariantQuantity ?? 0;
                if (variant.StockQuantity < requiredQty)
                    throw new ArgumentException($"'{terrarium.TerrariumName}' không đủ hàng. Còn: {variant.StockQuantity}, cần: {requiredQty}");
            }
        }
    }

    // ✅ CONSUME VOUCHER
    private async Task ConsumeVoucher(int voucherId, string userId, decimal orderAmount)
    {
        try
        {
            var voucher = await _unitOfWork.Voucher.GetByIdAsync(voucherId);
            if (voucher != null)
            {
                var result = await _unitOfWork.Voucher.ConsumeAsync(voucher.Code, userId, orderAmount);
                if (!result.ok)
                {
                    _logger?.LogWarning($"Failed to consume voucher {voucherId}: {result.message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Error consuming voucher {voucherId}: {ex.Message}");
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

    public async Task<IEnumerable<OrderSummaryResponse>> GetByUserAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId phải là số nguyên dương.", nameof(userId));

        var orders = await _unitOfWork.Order.FindByUserAsync(userId);

        return orders.Select(order => new OrderSummaryResponse
        {
            OrderId = order.OrderId,
            UserId = order.UserId,
            VoucherId = order.VoucherId,
            AddressId = order.AddressId,
            TotalAmount = order.TotalAmount,
            OriginalAmount = order.OriginalAmount, // ✅ THÊM
            DiscountAmount = order.DiscountAmount,   // ✅ THÊM
            Deposit = order.Deposit,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus ?? string.Empty,
            TransactionId = order.TransactionId,

        });
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
                order.Status = OrderStatusData.Refunded;
                refund.RefundAmount = request.RefundAmount;
            }
            else
            {
                order.Status = OrderStatusData.Completed;
            }
            var isRollback = false;
            try
            {
                await _unitOfWork.Order.UpdateAsync(order);
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
            if (item.ComboId < 0)
            {
                // KHÔNG cần check Quantity
                if ((item.AccessoryId == null && item.TerrariumVariantId == null)
                    || ((item.AccessoryQuantity ?? 0) <= 0 && (item.TerrariumVariantQuantity ?? 0) <= 0))
                    throw new ArgumentException("Item phải có AccessoryId hoặc TerrariumVariantId và quantity > 0.", nameof(r.Items));
            }
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
public class StockValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}