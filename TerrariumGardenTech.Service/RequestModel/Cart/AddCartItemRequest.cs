using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Service.RequestModel.Cart;

public class AddCartItemRequest
{
    [Required] public int? AccessoryId { get; set; } // Khóa ngoại đến Accessory, có thể null nếu là TerrariumVariant

    [Required]
    public int? TerrariumVariantId { get; set; } // Khóa ngoại đến TerrariumVariant, có thể null nếu là Accessory

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
    public int Quantity { get; set; } // Số lượng sản phẩm trong giỏ

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Giá đơn vị không hợp lệ.")]
    public decimal UnitPrice { get; set; } // Giá sản phẩm tại thời điểm thêm vào giỏ
}