namespace TerrariumGardenTech.Common.ResponseModel.Order;

/// <summary>Dữ liệu trả về cho từng mục hàng</summary>
public class OrderItemResponse
{
    public int OrderItemId { get; set; }
    public int? AccessoryId { get; set; }
    public int? TerrariumVariantId { get; set; }
    public int? AccessoryQuantity { get; set; } // Số lượng phụ kiện
    public int? TerrariumVariantQuantity { get; set; } // Số lượng biến thể terrarium
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
}