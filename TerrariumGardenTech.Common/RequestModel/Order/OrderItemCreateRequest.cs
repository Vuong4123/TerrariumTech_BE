namespace TerrariumGardenTech.Common.RequestModel.Order;

/// <summary>Payload tạo một mục hàng trong đơn</summary>
//public class OrderItemCreateRequest
//{
//    public int? AccessoryId { get; set; }
//    public int? TerrariumVariantId { get; set; }
//    public int? AccessoryQuantity { get; set; } // Số lượng phụ kiện
//    public int? TerrariumVariantQuantity { get; set; } // Số lượng terrarium
//    //public int Quantity { get; set; }
//    //public decimal UnitPrice { get; set; }
//}

public class OrderItemCreateRequest
{
    public string ItemType { get; set; }
    public int? TerrariumId { get; set; }
    public int? AccessoryId { get; set; }
    public int? TerrariumVariantId { get; set; }
    public int? AccessoryQuantity { get; set; }
    public int? TerrariumVariantQuantity { get; set; }
    public int? ComboId { get; set; }
    public int? ComboQuantity { get; set; }
}