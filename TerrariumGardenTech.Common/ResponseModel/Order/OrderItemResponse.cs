namespace TerrariumGardenTech.Common.ResponseModel.Order;

/// <summary>Dữ liệu trả về cho từng mục hàng</summary>
public class OrderItemResponse
{
    public int ComboId { get; set; } // ID của combo nếu có
    public string ItemType { get; set; } = "Single"; // Loại mục hàng, có thể là "Single" hoặc "Combo"
    public int OrderItemId { get; set; }
    public int? TerrariumId { get; set; }
    public int? AccessoryId { get; set; }
    public int? TerrariumVariantId { get; set; }
    public int? AccessoryQuantity { get; set; } // Số lượng phụ kiện
    public int? TerrariumVariantQuantity { get; set; } // Số lượng biến thể terrarium
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public int? ParentOrderItemId { get; set; }
    public List<OrderItemResponse> ChildItems { get; set; } = new List<OrderItemResponse>();

    // ✅ THÊM: Product details
    public string? ProductName { get; set; }
    public string? ImageUrl { get; set; }
}