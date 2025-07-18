namespace TerrariumGardenTech.Service.RequestModel.Order;

/// <summary>Payload tạo một mục hàng trong đơn</summary>
public class OrderItemCreateRequest
{
    public int? AccessoryId { get; set; }
    public int? TerrariumVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}