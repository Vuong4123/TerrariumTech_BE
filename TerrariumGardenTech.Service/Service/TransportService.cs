using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Transports;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using static TerrariumGardenTech.Common.Enums.CommonData;

namespace TerrariumGardenTech.Service.Service
{
    public class TransportService : ITransportService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IWalletServices _walletService;

        public TransportService(UnitOfWork unitOfWork, ICloudinaryService cloudinaryService, IWalletServices walletService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _walletService = walletService;
        }

        public async Task<(int, IEnumerable<OrderTransport>)> Paging(int? orderId, int? shipperId, TransportStatusEnum? status, bool? isRefund, int pageIndex = 1, int pageSize = 10)
        {
            var query = _unitOfWork.Transport.DbSet().AsNoTracking();
            if (orderId.HasValue)
                query = query.Where(x => x.OrderId == orderId.Value);
            if (shipperId.HasValue)
                query = query.Where(x => x.UserId == shipperId.Value);
            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);
            if (isRefund.HasValue)
                query = query.Where(x => x.IsRefund == isRefund.Value);
            var total = await query.CountAsync();
            var datas = await query.Include(x => x.TransportLogs).OrderByDescending(x => x.CreatedDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();
            return (total, datas.Select(x =>
            {
                if (x.TransportLogs == null || !x.TransportLogs.Any())
                    return x;

                var lastLog = x.TransportLogs.OrderByDescending(log => log.LogId).FirstOrDefault();
                x.Image = lastLog?.CheckinImage;
                x.TransportLogs = null;
                return x;
            }));
        }

        public async Task<IBusinessResult> CreateTransport(CreateTransportModel request, int currentUserId)
        {
            var users = await _unitOfWork.User.DbSet().AsNoTracking()
                .Where(x => x.UserId == request.UserId || x.UserId == currentUserId)
                .Select(x => new { x.UserId, x.Username }).ToArrayAsync();
            var assignUser = users.FirstOrDefault(x => x.UserId == request.UserId);
            var currentUser = users.FirstOrDefault(x => x.UserId == currentUserId);
            if ((request.UserId.HasValue && assignUser == null) || currentUser == null)
                return new BusinessResult(false, "Thông tin người dùng không hợp lệ!");

            var order = await _unitOfWork.Order.DbSet().Include(x => x.Transports).Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.OrderId == request.OrderId);
            if (order == null)
                return new BusinessResult(false, "Không tìm thấy thông tin đơn hàng!");
            if (order.Transports != null && (
                (order.Status == OrderStatusData.Processing && order.Transports.Any(x => x.Status == TransportStatusEnum.Shipping || x.Status == TransportStatusEnum.Completed)) ||
                (order.Status == OrderStatusData.Refuning && order.Transports.Any(x => x.Status == TransportStatusEnum.InCustomer))))
                return new BusinessResult(false, "Đơn hàng này đang có đơn vận chuyển, không thể tạo thêm");
            var verifyStatus = order.Status switch
            {
                OrderStatusData.Cancel => "Đơn hàng đã bị hủy, không thể thực hiện tạo đơn vận chuyển!",
                OrderStatusData.Pending => "Đơn hàng đang chờ xác nhận, không thể thực hiện vận chuyển!",
                OrderStatusData.Confirmed => "Đơn hàng chưa được hoàn thiện, không thể thực hiện vận chuyển!",
                OrderStatusData.Shipping => "Đơn hàng đang được vận chuyển!",
                OrderStatusData.Completed => "Đơn hàng đã giao thành công, không thể thực hiện tiếp!",
                OrderStatusData.RequestRefund => "Đơn hàng đang có yêu cầu hoàn tiện, không thể thực hiện tiếp!",
                OrderStatusData.Refunded => "Đơn hàng đã hoàn tiền, không thể thực hiện tiếp!",
                _ => string.Empty
            };
            if (order.Status == OrderStatusData.Shipping && request.IsRefund)
                return new BusinessResult(false, "Không thể tạo đơn hoàn tiền cho đơn hàng chưa giao!");
            if (order.Status == OrderStatusData.Refuning && !request.IsRefund)
                return new BusinessResult(false, "Không thể tạo đơn giao tới khách hàng cho đơn hàng hoàn tiền!");
            if (!string.IsNullOrEmpty(verifyStatus))
                return new BusinessResult(false, verifyStatus);

            OrderRequestRefund? refun = null;
            if (request.IsRefund)
            {
                refun = await _unitOfWork.OrderRequestRefund.DbSet()
                    .FirstOrDefaultAsync(x => x.OrderId == request.OrderId && x.Status == OrderStatusData.Approved);
                if (refun == null)
                    return new BusinessResult(false, "Không tìm thấy thông tin yêu cầu hoàn tiền cho đơn hàng này!");
            }

            using (var trans = await _unitOfWork.Transport.BeginTransactionAsync())
            {
                var isRollback = false;
                try
                { 
                    var transport = new OrderTransport
                    {
                        OrderId = request.OrderId,
                        Status = TransportStatusEnum.InWarehouse,
                        EstimateCompletedDate = request.EstimateCompletedDate,
                        Note = request.Note,
                        IsRefund = request.IsRefund,
                        UserId = assignUser?.UserId,
                        CreatedDate = DateTime.Now,
                        CreatedBy = currentUser.Username,
                        Items = order.OrderItems.Select(x => new OrderTransportItem
                        {
                            OrderItemId = x.OrderItemId,
                            Quantity = x.Quantity ?? 0
                        }).ToList()
                    };
                    if (order.Status == OrderStatusData.Processing)
                        order.Status = OrderStatusData.Shipping;
                    await _unitOfWork.Transport.CreateAsync(transport);
                    await _unitOfWork.SaveAsync();
                    if (refun != null)
                    {
                        refun.TransportId = transport.TransportId;
                        await _unitOfWork.OrderRequestRefund.UpdateAsync(refun);
                    }
                    await _unitOfWork.SaveAsync();
                    transport.Order = null;
                    transport.TransportLogs = null;
                    transport.Items = transport.Items.Select(x => { x.OrderTransport = null; return x; }).ToList();
                    return new BusinessResult(true, "Tạo đơn vận chuyển thành công!", transport);
                }
                catch (Exception)
                {
                    isRollback = true;
                    return new BusinessResult(false, "Tạo đơn vận chuyển thất bại!");
                }
                finally { 
                    if (isRollback)
                        await trans.RollbackAsync();
                    else
                        await trans.CommitAsync();
                }
            }
        }

        public async Task<IBusinessResult> DeleteTransport(int transportId)
        {
            var transport = await _unitOfWork.Transport.DbSet().FirstOrDefaultAsync(x => x.TransportId == transportId);
            if (transport == null)
                return new BusinessResult(false, "Không tìm thấy thông tin đơn vận chuyển!");
            if (transport.Status != TransportStatusEnum.InWarehouse)
                return new BusinessResult(false, "Không thể xóa đơn vận chuyển đã được giao hoặc đang vận chuyển!");

            var logs = await _unitOfWork.TransportLog.DbSet()
                .Where(x => x.OrderTransportId == transportId)
                .ToListAsync();
            var order = await _unitOfWork.Order.DbSet().FirstOrDefaultAsync(x => x.OrderId == transport.OrderId);
            await Task.WhenAll(logs.Select(_unitOfWork.TransportLog.RemoveAsync));
            if (order != null)
            {
                order.Status = transport.IsRefund ? OrderStatusData.Refunded : OrderStatusData.Processing;
                await _unitOfWork.Order.UpdateAsync(order);
            }    
            await _unitOfWork.Transport.RemoveAsync(transport);
            await _unitOfWork.SaveAsync();
            return new BusinessResult(true, "Xóa đơn vận chuyển thành công!");
        }

        public async Task<OrderTransport> GetById(int transportId)
        {
            var transport = await _unitOfWork.Transport.DbSet().AsNoTracking()
                .Include(x => x.TransportLogs).Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.TransportId == transportId);
            if (transport == null) return null;

            var orderItems = (await _unitOfWork.Order.DbSet().AsNoTracking()
                .Include(x => x.OrderItems).ThenInclude(x => x.Combo)
                .Include(x => x.OrderItems).ThenInclude(x => x.Accessory).ThenInclude(x => x.AccessoryImages)
                .Include(x => x.OrderItems).ThenInclude(x => x.TerrariumVariant)
                .FirstOrDefaultAsync(x => x.OrderId == transport.OrderId))
                ?.OrderItems;

            transport.TransportLogs = transport.TransportLogs.Select(x => { x.Transport = null; return x; }).OrderByDescending(x => x.CreatedDate).ToList();
            transport.Items = transport.Items.Select(item => {
                item.OrderTransport = null;

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
            return transport;
        }

        public async Task<IEnumerable<OrderTransport>> GetByOrderId(int orderId)
        {
            var transports = await _unitOfWork.Transport.DbSet().AsNoTracking()
                .Include(x => x.TransportLogs)
                .Where(x => x.OrderId == orderId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return transports.Select(x => { 
                x.Order = null;
                return x; 
            });
        }

        public async Task<IBusinessResult> UpdateTransport(UpdateTransportModel request, int currentUserId)
        {
            var transport = await _unitOfWork.Transport.DbSet().FirstOrDefaultAsync(x => x.TransportId == request.TransportId);
            if (transport == null)
                return new BusinessResult(false, "Không tìm thấy thông tin đơn vận chuyển!");
            var order = await _unitOfWork.Order.DbSet().FirstOrDefaultAsync(x => x.OrderId == transport.OrderId);
            if (order == null)
                return new BusinessResult(false, "Không tìm thấy thông tin đơn hàng!");
            if (transport.Status == TransportStatusEnum.Completed || transport.Status == TransportStatusEnum.Failed || transport.Status == TransportStatusEnum.LostShipping || transport.Status == TransportStatusEnum.LostInWarehouse || transport.Status == TransportStatusEnum.CompletedToWareHouse || transport.Status == TransportStatusEnum.FailedToWareHouse || request.Status == TransportStatusEnum.GetFromCustomerFail)
                return new BusinessResult(false, "Không thể cập nhật trạng thái đơn vận chuyển!");

            if ((request.Status == TransportStatusEnum.Shipping || request.Status == TransportStatusEnum.Completed || request.Status == TransportStatusEnum.ShippingToWareHouse || request.Status == TransportStatusEnum.CompletedToWareHouse) && request.Image == null)
                return new BusinessResult(false, "Cần có hình ảnh checkin khi chuyển sang trạng thái này!");
            if (request.Image != null)
            {
                var allowExtension = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var extension = Path.GetExtension(request.Image.FileName);
                if (!allowExtension.Contains(extension))
                    return new BusinessResult(false, "Chỉ được chọn hình ảnh checkin!");
            }
            if ((request.Status == TransportStatusEnum.Failed || request.Status == TransportStatusEnum.FailedToWareHouse || request.Status == TransportStatusEnum.LostShipping || request.Status == TransportStatusEnum.LostInWarehouse || request.Status == TransportStatusEnum.GetFromCustomerFail) && string.IsNullOrEmpty(request.Reason))
                return new BusinessResult(false, "Vui lòng nhập lý do chuyển trạng thái!");

            if (transport.Status != request.Status && 
              (transport.Status == TransportStatusEnum.InWarehouse && request.Status != TransportStatusEnum.LostInWarehouse && request.Status != TransportStatusEnum.Shipping) ||
              (transport.Status == TransportStatusEnum.Shipping && request.Status != TransportStatusEnum.LostShipping && request.Status != TransportStatusEnum.Completed && request.Status != TransportStatusEnum.Failed) ||
              (transport.Status == TransportStatusEnum.InCustomer && request.Status != TransportStatusEnum.GetFromCustomerFail && request.Status != TransportStatusEnum.ShippingToWareHouse) ||
              (transport.Status == TransportStatusEnum.ShippingToWareHouse && request.Status != TransportStatusEnum.CompletedToWareHouse && request.Status != TransportStatusEnum.FailedToWareHouse))
                return new BusinessResult(false, "Trạng thái cập nhập không chính xác");
            if ((request.Status == TransportStatusEnum.Failed || request.Status == TransportStatusEnum.GetFromCustomerFail) && !request.ContactFailNumber.HasValue)
                return new BusinessResult(false, "Vui lòng nhập số lượt đã liên hệ thất bại tới khách hàng!");

            var users = await _unitOfWork.User.DbSet().AsNoTracking()
                .Where(x => x.UserId == request.AssignToUserId || x.UserId == currentUserId)
                .Select(x => new { x.UserId, x.Username }).ToArrayAsync();
            var assignUser = users.FirstOrDefault(x => x.UserId == request.AssignToUserId);
            var currentUser = users.FirstOrDefault(x => x.UserId == currentUserId);
            if ((request.AssignToUserId.HasValue && assignUser == null) || currentUser == null)
                return new BusinessResult(false, "Thông tin người dùng không hợp lệ!");

            var uploadResult = await _cloudinaryService.UploadImageAsync(request.Image, "Transport_checkin");
            var log = new TransportLog
            {
                OrderTransportId = transport.TransportId,
                LastStatus = transport.Status,
                NewStatus = request.Status,
                OldUser = transport.UserId,
                Reason = request.Reason,
                CheckinImage = uploadResult.Data?.ToString(),
                CreatedDate = DateTime.Now,
                CreatedBy = currentUser.Username
            };
            if (transport.Status == TransportStatusEnum.Failed || transport.Status == TransportStatusEnum.FailedToWareHouse || transport.Status == TransportStatusEnum.LostShipping || transport.Status == TransportStatusEnum.LostInWarehouse || transport.Status == TransportStatusEnum.Completed)
            {
                log.CurrentUser = null;
            }
            else
            {
                log.CurrentUser = request.AssignToUserId ?? transport.UserId;
            }
            transport.Status = request.Status;
            transport.UserId = request.AssignToUserId ?? transport.UserId;
            using (var trans = await _unitOfWork.Transport.BeginTransactionAsync())
            {
                try
                {
                    if (request.Status == TransportStatusEnum.Failed || request.Status == TransportStatusEnum.GetFromCustomerFail)
                        transport.ContactFailNumber = request.ContactFailNumber.Value;
                    if (transport.Status == TransportStatusEnum.Failed || transport.Status == TransportStatusEnum.LostShipping)
                        order.Status = OrderStatusData.Failed;
                    if (transport.Status == TransportStatusEnum.Completed || transport.Status == TransportStatusEnum.GetFromCustomerFail)
                        order.Status = OrderStatusData.Completed;
                    if (transport.Status == TransportStatusEnum.ShippingToWareHouse)
                    {
                        var refund = await _unitOfWork.OrderRequestRefund.DbSet()
                            .AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.TransportId == transport.TransportId);
                        if (refund != null && refund.Items != null && refund.Items.Any())
                        {
                            var point = refund.Items.Sum(x => x.RefundPoint);
                            await _walletService.RefundAsync(order.UserId, point, order.OrderId);
                        }
                    }
                    await _unitOfWork.TransportLog.CreateAsync(log);
                    await _unitOfWork.Transport.UpdateAsync(transport);
                    await _unitOfWork.Order.UpdateAsync(order);
                    await _unitOfWork.SaveAsync();
                    transport.Order = null;
                    transport.TransportLogs = null;
                    await trans.CommitAsync();
                    return new BusinessResult(true, "Cập nhật thông tin đơn vận chuyển thành công!", transport);
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    return new BusinessResult(false, "Cập nhập thông tin đơn vận chuyển thất bại!");
                }
            }
        }
    }
}
