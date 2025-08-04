using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Transports;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class TransportService : ITransportService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public TransportService(UnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<(bool, string)> CreateTransport(CreateTransportModel request, int currentUserId)
        {
            var users = await _unitOfWork.User.DbSet().AsNoTracking()
                .Where(x => x.UserId == request.UserId || x.UserId == currentUserId)
                .Select(x => new { x.UserId, x.Username }).ToArrayAsync();
            var assignUser = users.FirstOrDefault(x => x.UserId == request.UserId);
            var currentUser = users.FirstOrDefault(x => x.UserId == currentUserId);
            if ((request.UserId.HasValue && assignUser == null) || currentUser == null)
                return (false, "Thông tin người dùng không hợp lệ!");

            var order = await _unitOfWork.Order.DbSet().Include(x => x.Transports).FirstOrDefaultAsync(x => x.OrderId == request.OrderId);
            if (order == null)
                return (false, "Không tìm thấy thông tin đơn hàng!");
            if (order.Transports != null && (
                (order.Status == OrderStatusEnum.Processing && order.Transports.Any(x => x.Status == TransportStatusEnum.Shipping || x.Status == TransportStatusEnum.Completed)) ||
                (order.Status == OrderStatusEnum.Refuning && order.Transports.Any(x => x.Status == TransportStatusEnum.InCustomer))))
                return (false, "Đơn hàng này đang có đơn vận chuyển, không thể tạo thêm");
            var verifyStatus = order.Status switch
            {
                OrderStatusEnum.Cancle => "Đơn hàng đã bị hủy, không thể thực hiện tạo đơn vận chuyển!",
                OrderStatusEnum.Pending => "Đơn hàng đang chờ xác nhận, không thể thực hiện vận chuyển!",
                OrderStatusEnum.Confirmed => "Đơn hàng chưa được hoàn thiện, không thể thực hiện vận chuyển!",
                OrderStatusEnum.Shipping => "Đơn hàng đang được vận chuyển!",
                OrderStatusEnum.Completed => "Đơn hàng đã giao thành công, không thể thực hiện tiếp!",
                OrderStatusEnum.RequestRefund => "Đơn hàng đang có yêu cầu hoàn tiện, không thể thực hiện tiếp!",
                OrderStatusEnum.Refunded => "Đơn hàng đã hoàn tiền, không thể thực hiện tiếp!",
                _ => string.Empty
            };
            if (order.Status == OrderStatusEnum.Shipping && request.IsRefund)
                return (false, "Không thể tạo đơn hoàn tiền cho đơn hàng chưa giao!");
            if (order.Status == OrderStatusEnum.Refuning && !request.IsRefund)
                return (false, "Không thể tạo đơn giao tới khách hàng cho đơn hàng hoàn tiền!");
            if (!string.IsNullOrEmpty(verifyStatus))
                return (false, verifyStatus);

            OrderRequestRefund? refun = null;
            if (request.IsRefund)
            {
                refun = await _unitOfWork.OrderRequestRefund.DbSet()
                    .FirstOrDefaultAsync(x => x.OrderId == request.OrderId && x.Status == RequestRefundStatusEnum.Approved);
                if (refun == null)
                    return (false, "Không tìm thấy thông tin yêu cầu hoàn tiền cho đơn hàng này!");
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
                        CreatedBy = currentUser.Username
                    };
                    if (order.Status == OrderStatusEnum.Processing)
                        order.Status = OrderStatusEnum.Shipping;
                    await _unitOfWork.Transport.CreateAsync(transport);
                    await _unitOfWork.SaveAsync();
                    if (refun != null)
                    {
                        refun.TransportId = transport.TransportId;
                        await _unitOfWork.OrderRequestRefund.UpdateAsync(refun);
                    }
                    await _unitOfWork.SaveAsync();
                }
                catch (Exception)
                {
                    isRollback = true;
                    return (true, "Tạo đơn vận chuyển thất bại!");
                }
                finally { 
                    if (isRollback)
                        await trans.RollbackAsync();
                    else
                        await trans.CommitAsync();
                }
            }
            return (true, "Tạo đơn vận chuyển thành công!");
        }

        public async Task<(bool, string)> DeleteTransport(int transportId)
        {
            var transport = await _unitOfWork.Transport.DbSet().FirstOrDefaultAsync(x => x.TransportId == transportId);
            if (transport == null)
                return (false, "Không tìm thấy thông tin đơn vận chuyển!");
            if (transport.Status != TransportStatusEnum.InWarehouse)
                return (false, "Không thể xóa đơn vận chuyển đã được giao hoặc đang vận chuyển!");

            var logs = await _unitOfWork.TransportLog.DbSet()
                .Where(x => x.OrderTransportId == transportId)
                .ToListAsync();

            await Task.WhenAll(logs.Select(_unitOfWork.TransportLog.RemoveAsync));
            await _unitOfWork.Transport.RemoveAsync(transport);
            await _unitOfWork.SaveAsync();
            return (true, "Xóa đơn vận chuyển thành công!");
        }

        public async Task<OrderTransport> GetById(int transportId)
        {
            var transport = await _unitOfWork.Transport.DbSet().AsNoTracking()
                .Include(x => x.TransportLogs).FirstOrDefaultAsync(x => x.TransportId == transportId);
            if (transport == null) return null;

            transport.TransportLogs = transport.TransportLogs.Select(x => { x.Transport = null; return x; }).OrderByDescending(x => x.CreatedDate).ToList();
            return transport;
        }

        public async Task<IEnumerable<OrderTransport>> GetByOrderId(int orderId)
        {
            var transports = await _unitOfWork.Transport.DbSet().AsNoTracking()
                .Where(x => x.OrderId == orderId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
            return transports.Select(x => { 
                x.Order = null;
                return x; 
            });
        }

        public async Task<(bool, string)> UpdateTransport(UpdateTransportModel request, int currentUserId)
        {
            var transport = await _unitOfWork.Transport.DbSet().FirstOrDefaultAsync(x => x.TransportId == request.TransportId);
            if (transport == null)
                return (false, "Không tìm thấy thông tin đơn vận chuyển!");
            var order = await _unitOfWork.Order.DbSet().FirstOrDefaultAsync(x => x.OrderId == transport.OrderId);
            if (order == null)
                return (false, "Không tìm thấy thông tin đơn hàng!");
            if (transport.Status == TransportStatusEnum.Completed || transport.Status == TransportStatusEnum.Failed || transport.Status == TransportStatusEnum.LostShipping || transport.Status == TransportStatusEnum.LostInWarehouse || transport.Status == TransportStatusEnum.CompletedToWareHouse || transport.Status == TransportStatusEnum.FailedToWareHouse || request.Status == TransportStatusEnum.GetFromCustomerFail)
                return (false, "Không thể cập nhật trạng thái đơn vận chuyển!");

            if ((request.Status == TransportStatusEnum.Shipping || request.Status == TransportStatusEnum.Completed || request.Status == TransportStatusEnum.ShippingToWareHouse || request.Status == TransportStatusEnum.CompletedToWareHouse) && request.Image == null)
                return (false, "Cần có hình ảnh checkin khi chuyển sang trạng thái này!");
            if (request.Image != null)
            {
                var allowExtension = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var extension = Path.GetExtension(request.Image.FileName);
                if (!allowExtension.Contains(extension))
                    return (false, "Chỉ được chọn hình ảnh checkin!");
            }
            if ((request.Status == TransportStatusEnum.Failed || request.Status == TransportStatusEnum.FailedToWareHouse || request.Status == TransportStatusEnum.LostShipping || request.Status == TransportStatusEnum.LostInWarehouse || request.Status == TransportStatusEnum.GetFromCustomerFail) && string.IsNullOrEmpty(request.Reason))
                return (false, "Vui lòng nhập lý do chuyển trạng thái!");

            if (transport.Status != request.Status && 
              (transport.Status == TransportStatusEnum.InWarehouse && request.Status != TransportStatusEnum.LostInWarehouse && request.Status != TransportStatusEnum.Shipping) ||
              (transport.Status == TransportStatusEnum.Shipping && request.Status != TransportStatusEnum.LostShipping && request.Status != TransportStatusEnum.Completed && request.Status != TransportStatusEnum.Failed) ||
              (transport.Status == TransportStatusEnum.InCustomer && request.Status != TransportStatusEnum.GetFromCustomerFail && request.Status != TransportStatusEnum.ShippingToWareHouse) ||
              (transport.Status == TransportStatusEnum.ShippingToWareHouse && request.Status != TransportStatusEnum.CompletedToWareHouse && request.Status != TransportStatusEnum.FailedToWareHouse))
                return (false, "Trạng thái cập nhập không chính xác");
            if ((request.Status == TransportStatusEnum.Failed || request.Status == TransportStatusEnum.GetFromCustomerFail) && !request.ContactFailNumber.HasValue)
                return (false, "Vui lòng nhập số lượt đã liên hệ thất bại tới khách hàng!");

            var users = await _unitOfWork.User.DbSet().AsNoTracking()
                .Where(x => x.UserId == request.AssignToUserId || x.UserId == currentUserId)
                .Select(x => new { x.UserId, x.Username }).ToArrayAsync();
            var assignUser = users.FirstOrDefault(x => x.UserId == request.AssignToUserId);
            var currentUser = users.FirstOrDefault(x => x.UserId == currentUserId);
            if ((request.AssignToUserId.HasValue && assignUser == null) || currentUser == null)
                return (false, "Thông tin người dùng không hợp lệ!");

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
            if (request.Status == TransportStatusEnum.Failed || request.Status == TransportStatusEnum.GetFromCustomerFail)
                transport.ContactFailNumber = request.ContactFailNumber.Value;
            if (transport.Status == TransportStatusEnum.Failed || transport.Status == TransportStatusEnum.LostShipping)
                order.Status = OrderStatusEnum.Failed;
            if (transport.Status == TransportStatusEnum.Completed || transport.Status == TransportStatusEnum.GetFromCustomerFail)
                order.Status = OrderStatusEnum.Completed;
            if (transport.Status == TransportStatusEnum.ShippingToWareHouse)
                order.Status = OrderStatusEnum.Refunded;
            await _unitOfWork.TransportLog.CreateAsync(log);
            await _unitOfWork.Transport.UpdateAsync(transport);
            await _unitOfWork.Order.UpdateAsync(order);
            await _unitOfWork.SaveAsync();
            return (true, "Cập nhật thông tin đơn vận chuyển thành công!");
        }
    }
}
