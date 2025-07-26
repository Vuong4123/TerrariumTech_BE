using System.Text.Json.Serialization;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity;

public class CartItem
{
    public int CartItemId { get; set; } // Khóa chính
    public int CartId { get; set; } // Khóa ngoại đến Cart

    [JsonIgnore] // Không tuần tự hóa thuộc tính này để tránh chu trình
    public Cart Cart { get; set; }

    // Khóa ngoại đến sản phẩm (Accessory hoặc TerrariumVariant)
    public int? AccessoryId { get; set; }
    public Accessory Accessory { get; set; }
    public int? AccessoryQuantity { get; set; }
    public int? TerrariumVariantQuantity { get; set; }

    public int? TerrariumVariantId { get; set; }
    public TerrariumVariant TerrariumVariant { get; set; }

    public int Quantity { get; set; } // Số lượng sản phẩm trong giỏ
    public decimal UnitPrice { get; set; } // Giá sản phẩm tại thời điểm thêm vào giỏ
    public decimal TotalPrice { get; set; } // Tính tổng tiền của sản phẩm trong giỏ

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}