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
            Status = OrderStatusEnum.Pending,
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


    public async Task<(bool, string, object?)> RequestRefundAsync(CreateRefundRequest request, int currentUserId)
    {
        var order = await _unitOfWork.Order.DbSet().Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.OrderId == request.OrderId && x.UserId == currentUserId);
        if (order == null)
            return (false, "Không tìm thấy thông tin hóa đơn!", null);

        if (order.Status != OrderStatusEnum.Completed)
            return (false, "Đơn hàng này không cho phép hoàn tiền!", null);

        if (request.RefundItems == null || !request.RefundItems.Any())
            return (false, "Vui lòng chọn sản phẩm cần hoàn tiền!", null);

        if (request.RefundItems.Any(x => x.Quantity <= 0))
            return (false, "Số lượng sản phẩm yêu cầu hoàn tiền phải lớn hơn 0!", null);

        if (request.RefundItems.Any(x => string.IsNullOrEmpty(x.Reason)))
            return (false, "Vui lòng nhập đầy đủ lý do hoàn tiền!", null);

        if (request.RefundItems.Any(x => !order.OrderItems.Any(i => i.OrderItemId == x.OrderItemId)))
            return (false, "Danh sách sản phẩm cần hoàn tiền không chính xác!", null);

        if (request.RefundItems.Any(x => order.OrderItems.Any(i => x.OrderItemId == x.OrderItemId && i.Quantity < x.Quantity)))
            return (false, "Số lượng sản phẩm yêu cầu hoàn tiền vượt quá số lượng đã đặt trong đơn hàng!", null);

        var existedRefundItems = await _unitOfWork.OrderRequestRefund.DbSet()
            .Include(x => x.Items).Where(x => x.OrderId == order.OrderId && x.Status == RequestRefundStatusEnum.Approved)
            .SelectMany(x => x.Items).Where(x => x.IsApproved == true).ToArrayAsync();
        if (existedRefundItems.Any())
        {
            var checkQuantity = request.RefundItems.Select(x =>
            {
                // Số lượng đã từng yêu cầu hoàn hàng
                var exited = existedRefundItems.FirstOrDefault(item => x.OrderItemId == item.OrderItemId)?.Quantity ?? 0;
                // Số lượng tối đa cho hoàn hàng
                var maxQuantity = order.OrderItems.FirstOrDefault(item => x.OrderItemId == item.OrderItemId)?.Quantity ?? 0;
                return maxQuantity < x.Quantity + exited;
            });
            // Nếu có thằng nào có tổng số lượng yêu cầu refund > số lượng đã đặt trong đơn hàng
            if (checkQuantity.Any(x => x))
                return (false, "Số lượng sản phẩm yêu cầu hoàn tiền vượt quá số lượng đã đặt trong đơn hàng!", null);
        }

        var refundRequest = new OrderRequestRefund
        {
            OrderId = order.OrderId,
            Status = RequestRefundStatusEnum.Waiting,
            RequestDate = DateTime.Today,
            Items = request.RefundItems.Select(x => new OrderRefundItem
            {
                OrderItemId = x.OrderItemId,
                Quantity = x.Quantity
            }).ToList(),
        };
        order.Status = OrderStatusEnum.RequestRefund;
        await _unitOfWork.OrderRequestRefund.CreateAsync(refundRequest);
        await _unitOfWork.SaveAsync();
        refundRequest.Order = null;
        refundRequest.Items = refundRequest.Items.Select(x => { x.OrderRefund = null; return x; }).ToList();
        return (true, "Yêu cầu hoàn tiền đã được gửi thành công!", refundRequest);
    }

    public async Task<IBusinessResult> GetRefundAsync(int orderId)
    {
        var order = await _unitOfWork.Order.DbSet().Include(x => x.Refunds).AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (order == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Không tìm thấy thông tin hóa đơn!", null);

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách yêu cầu hoàn tiền thành công!", order.Refunds.Select(x =>
        {
            x.Order = null;
            return x;
        }).ToArray());
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
        if (refund.Order?.UserId != currentUserId && request.Status != RequestRefundStatusEnum.Cancle)
            return (false, "Bạn không có quyền thực hiện thao tác này!", null);

        //if (request.Status == RequestRefundStatusEnum.Waiting)
        //    return (false, "Trạng thái cho đơn yêu cầu hoàn tiền không hợp lệ!", null);

        if ((request.Status == RequestRefundStatusEnum.Cancle || request.Status == RequestRefundStatusEnum.Approved || request.Status == RequestRefundStatusEnum.Rejected) && refund.Status != RequestRefundStatusEnum.Waiting)
            return (false, "Yêu cầu hoàn tiền này không thể bị chỉnh sửa nữa!", null);

        if (request.Items == null || !request.Items.Any())
            return (false, "Thông tin sản phẩm yêu cầu hoàn tiền không chính xác!", null);

        if (request.Items.Any(x => x.Point <= 0))
            return (false, "Số tiền hoàn lại hoặc số điểm hoàn lại phải lớn hơn 0!", null);

        foreach (var item in refund.Items)
        {
            var itemEdit = request.Items.FirstOrDefault(x => x.OrderRefundItemId == item.OrderRefundItemId);
            if (itemEdit == null) continue;
            if (!itemEdit.IsApproved && string.IsNullOrEmpty(itemEdit.Reason))
                return (false, "Vui lòng nhập lý do không hoàn tiền cho sản phẩm!", null);

            item.IsApproved = itemEdit.IsApproved;
            item.ReasonModified = itemEdit.Reason;
            item.RefundPoint = itemEdit.Point;
            item.UserModified = currentUserId;
        }

        if ((request.Status == RequestRefundStatusEnum.Approved || request.Status == RequestRefundStatusEnum.Rejected) && refund.Items.Any(x => !x.IsApproved.HasValue))
            return (false, "Vui lòng cập nhập trạng thái cho tất cả các sản phẩm yêu cầu hoàn tiền trước khi chuyển trạng thái đơn yêu cầu!", null);

        if (request.Status == RequestRefundStatusEnum.Rejected && refund.Items.Any(x => x.IsApproved == true))
            return (false, "Đã có sản phẩm được duyệt hoàn tiền, không thể chuyển trạng thái từ chối cho đơn yêu cầu!", null);

        OrderTransport? transport = null;
        if (request.Status == RequestRefundStatusEnum.Approved && request.CreateTransport && refund.Items.Any(x => x.IsApproved == true))
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
            refund.LastModifiedDate = DateTime.Now;
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