using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.Cart
{
    public class CartItemResponse
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }

        // Thêm các thuộc tính cho Accessory
        public int? AccessoryId { get; set; }
        public string Accessory { get; set; }  // Thêm tên phụ kiện nếu có
        public int AccessoryQuantity { get; set; }  // Số lượng cho phụ kiện
        public decimal AccessoryUnitPrice { get; set; }  // Giá của phụ kiện

        // Thêm các thuộc tính cho TerrariumVariant
        public int? TerrariumVariantId { get; set; }
        public string TerrariumVariant { get; set; }  // Thêm tên biến thể terrarium nếu có
        public int TerrariumVariantQuantity { get; set; }  // Số lượng cho variant
        public decimal TerrariumVariantUnitPrice { get; set; }  // Giá của variant

        // Tổng giá trị sản phẩm trong giỏ
        public decimal TotalPrice { get; set; }

        // Thêm thông tin thời gian
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Thêm thông tin về UnitPrice và Quantity tổng quát
        public decimal UnitPrice { get; set; }  // Giá của món hàng
        public int Quantity { get; set; }  // Số lượng của món hàng
    }


}
