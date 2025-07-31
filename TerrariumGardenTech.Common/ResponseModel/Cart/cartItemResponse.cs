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
        public int? AccessoryId { get; set; }
        public int? TerrariumVariantId { get; set; }
        public List<CartItemDetail> Item { get; set; } // Danh sách các item (Accessory hoặc TerrariumVariant)
        public int TotalCartQuantity { get; set; }
        public decimal TotalCartPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class CartItemDetail
    {
        public string ProductName { get; set; } // Tên phụ kiện hoặc Terrarium
        public int Quantity { get; set; } // Số lượng
        public decimal Price { get; set; } // Giá
        public decimal TotalPrice { get; set; } // Tổng giá trị
    }

    public class CartResponse
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public string User { get; set; } // Tên người dùng hoặc email
        public List<CartItemResponse> CartItems { get; set; }
        public int TotalCartQuantity { get; set; }
        public decimal TotalCartPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
