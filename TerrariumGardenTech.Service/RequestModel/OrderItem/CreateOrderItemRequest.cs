namespace TerrariumGardenTech.Service.RequestModel.OrderItem;

/// <summary>Payload tạo mới một mục OrderItem riêng lẻ</summary>
public class CreateOrderItemRequest
{
    /// <summary>ID đơn hàng mà mục này thuộc về</summary>
    public int OrderId { get; set; }

    /// <summary>ID phụ kiện (nếu là accessory item)</summary>
    public int? AccessoryId { get; set; }

    /// <summary>ID biến thể Terrarium (nếu là terrarium item)</summary>
    public int? TerrariumVariantId { get; set; }

    /// <summary>Số lượng</summary>
    public int Quantity { get; set; }

    /// <summary>Đơn giá</summary>
    public decimal UnitPrice { get; set; }
}