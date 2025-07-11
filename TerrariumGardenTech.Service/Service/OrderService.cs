using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;                                   // cho IEnumerable.Select(...)
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Repositories;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Order;
using TerrariumGardenTech.Service.ResponseModel.Order;
using TerrariumGardenTech.Service.ResponseModel.OrderItem;


namespace TerrariumGardenTech.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly OrderRepository _repo;
        private readonly ILogger<OrderService> _logger;

        public OrderService(OrderRepository repo, ILogger<OrderService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<OrderResponse>> GetAllAsync()
        {
            _logger.LogInformation("Lấy danh sách tất cả đơn hàng");
            var orders = await _repo.GetAllAsync();
            return orders.Select(ToResponse);
        }

        public async Task<OrderResponse?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));

            var order = await _repo.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId}", id);
                return null;
            }
            return ToResponse(order);
        }

        public async Task<int> CreateAsync(OrderCreateRequest r)
        {
            ValidateCreateRequest(r);
            var entity = MapToEntity(r);

            try
            {
                await _repo.CreateAsync(entity);
                _logger.LogInformation("Tạo đơn hàng {OrderId} cho user {UserId}", entity.OrderId, r.UserId);
                return entity.OrderId;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng cho user {UserId}", r.UserId);
                throw new InvalidOperationException("Không thể tạo đơn hàng, vui lòng thử lại.");
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            if (id <= 0)
                throw new ArgumentException("OrderId phải là số nguyên dương.", nameof(id));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status không được để trống.", nameof(status));

            var order = await _repo.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId} để cập nhật", id);
                return false;
            }

            try
            {
                order.Status = status.Trim();
                await _repo.UpdateAsync(order);
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

            var order = await _repo.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng với ID {OrderId} để xóa", id);
                return false;
            }

            try
            {
                await _repo.RemoveAsync(order);
                _logger.LogInformation("Xóa đơn hàng {OrderId}", id);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa đơn hàng {OrderId}", id);
                throw new InvalidOperationException("Không thể xóa đơn hàng, vui lòng thử lại.");
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

        private static Order MapToEntity(OrderCreateRequest r) => new()
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

        private static OrderResponse ToResponse(Order o) => new()
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
        #endregion
    }
}
