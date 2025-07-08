using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.ResponseModel.Order
{
    /// <summary>Dữ liệu trả về khi truy vấn đơn hàng</summary>
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? OrderDate { get; set; }

        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string ShippingStatus { get; set; } = string.Empty;

        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
