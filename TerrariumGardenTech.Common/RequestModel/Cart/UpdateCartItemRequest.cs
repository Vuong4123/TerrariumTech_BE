namespace TerrariumGardenTech.Common.RequestModel.Cart;

public class UpdateCartItemRequest
{
    public int? AccessoryQuantity { get; set; } // Số lượng cho phụ kiện
    public int? VariantQuantity { get; set; } // Số lượng cho variant
    public int? Quantity { get; set; } // Số lượng cho combo hoặc general quantity
}
