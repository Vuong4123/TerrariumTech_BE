using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.Order
{
    /// <summary>Payload tạo đơn hàng</summary>
    public class OrderCreateRequest
    {
        public int UserId { get; set; }
        public int? VoucherId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? Deposit { get; set; }

        // Danh sách mặt hàng
        public List<OrderItemCreateRequest> Items { get; set; } = new();
    }
}
