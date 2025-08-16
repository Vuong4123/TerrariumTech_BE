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

    /// <summary>
    /// Response cho một bundle (bể + phụ kiện kèm theo)
    /// </summary>
    public class CartBundleResponse
    {
        /// <summary>
        /// Thông tin bể thủy sinh chính
        /// </summary>
        public CartItemResponse MainItem { get; set; }

        /// <summary>
        /// Danh sách phụ kiện kèm theo bể
        /// </summary>
        public List<CartItemResponse> BundleAccessories { get; set; } = new();

        /// <summary>
        /// Tổng tiền của cả bundle (bể + tất cả phụ kiện)
        /// </summary>
        public decimal TotalBundlePrice { get; set; }

        /// <summary>
        /// Tổng số lượng sản phẩm trong bundle
        /// </summary>
        public int TotalBundleQuantity { get; set; }

        /// <summary>
        /// Tự động tính toán tổng tiền và số lượng
        /// </summary>
        public void UpdateTotals()
        {
            TotalBundlePrice = (MainItem?.TotalCartPrice ?? 0) +
                              BundleAccessories.Sum(x => x.TotalCartPrice);

            TotalBundleQuantity = (MainItem?.TotalCartQuantity ?? 0) +
                                 BundleAccessories.Sum(x => x.TotalCartQuantity);
        }
    }

    /// <summary>
    /// Response cho toàn bộ giỏ hàng
    /// </summary>
    public class CartResponse
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public string User { get; set; }

        /// <summary>
        /// Danh sách các bundle (bể + phụ kiện kèm theo)
        /// </summary>
        public List<CartBundleResponse> BundleItems { get; set; } = new();

        /// <summary>
        /// Danh sách sản phẩm mua riêng lẻ (không thuộc bundle nào)
        /// </summary>
        public List<CartItemResponse> SingleItems { get; set; } = new();

        /// <summary>
        /// Tổng tiền toàn bộ giỏ hàng
        /// </summary>
        public decimal TotalCartPrice { get; set; }

        /// <summary>
        /// Tổng số lượng sản phẩm trong giỏ
        /// </summary>
        public int TotalCartQuantity { get; set; }

        /// <summary>
        /// Tổng số loại sản phẩm khác nhau
        /// </summary>
        public int TotalCartItem { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


}
