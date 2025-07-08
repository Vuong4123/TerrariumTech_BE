using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Repositories;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Order;
using TerrariumGardenTech.Service.ResponseModel.Order;
using System.Linq;                                   // cho IEnumerable.Select(...)
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly OrderRepository _repo;

        public OrderService(OrderRepository repo) => _repo = repo;

        /*----------- READ -----------*/
        public async Task<IEnumerable<OrderResponse>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(ToResponse);

        public async Task<OrderResponse?> GetByIdAsync(int id)
        {
            var order = await _repo.GetByIdAsync(id);
            return order is null ? null : ToResponse(order);
        }

        /*----------- CREATE -----------*/
        public async Task<int> CreateAsync(OrderCreateRequest r)
        {
            var entity = MapToEntity(r);
            await _repo.CreateAsync(entity);
            return entity.OrderId;
        }

        /*----------- UPDATE STATUS -----------*/
        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order is null) return false;

            order.Status = status;
            await _repo.UpdateAsync(order);
            return true;
        }

        /*----------- DELETE -----------*/
        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order is null) return false;
            await _repo.RemoveAsync(order);
            return true;
        }

        /*----------- Mapping helpers -----------*/
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
                TotalPrice = i.UnitPrice * i.Quantity
            }).ToList()
        };

        private static OrderResponse ToResponse(Order o) => new()
        {
            OrderId = o.OrderId,
            UserId = o.UserId,
            TotalAmount = o.TotalAmount,
            OrderDate = o.OrderDate,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            ShippingStatus = o.ShippingStatus,
            Items = o.OrderItems.Select(i => new OrderItemResponse
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
}
