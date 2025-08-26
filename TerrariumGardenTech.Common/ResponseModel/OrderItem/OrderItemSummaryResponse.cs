namespace TerrariumGardenTech.Common.ResponseModel.OrderItem;

/// <summary>Dữ liệu trả về cho từng OrderItem</summary>
public class OrderItemSummaryResponse
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int? TerrariumId { get; set; }
    public int? AccessoryId { get; set; }
    public int? TerrariumVariantId { get; set; }
    public int? AccessoryQuantity { get; set; } // Số lượng phụ kiện
    public int? TerrariumVariantQuantity { get; set; } // Số lượng terrarium
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}