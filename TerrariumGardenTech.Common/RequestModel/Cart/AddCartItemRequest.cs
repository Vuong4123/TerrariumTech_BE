using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Common.RequestModel.Cart;

public class AddCartItemRequest
{
    // Các tham số truyền vào giỏ hàng
    public int? AccessoryId { get; set; } // Phụ kiện, có thể null nếu chỉ là variant
    public int? TerrariumVariantId { get; set; } // Variant, có thể null nếu chỉ là phụ kiện

    public int? AccessoryQuantity { get; set; } // Số lượng cho phụ kiện, có thể null nếu không mua phụ kiện
    public int? VariantQuantity { get; set; } // Số lượng cho variant, có thể null nếu không mua variant
}


