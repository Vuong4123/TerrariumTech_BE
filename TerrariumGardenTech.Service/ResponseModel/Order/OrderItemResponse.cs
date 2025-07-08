using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.ResponseModel.Order
{
    /// <summary>Dữ liệu trả về cho từng mục hàng</summary>
    public class OrderItemResponse
    {
        public int OrderItemId { get; set; }
        public int? AccessoryId { get; set; }
        public int? TerrariumVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
