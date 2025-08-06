namespace TerrariumGardenTech.Common.RequestModel.Order;

/// <summary>Payload tạo đơn hàng</summary>
public class OrderCreateRequest
{
    public int UserId { get; set; }
    public int? VoucherId { get; set; }
    //public decimal TotalAmount { get; set; }
    public decimal? Deposit { get; set; }

    // Danh sách mặt hàng
    public List<OrderItemCreateRequest> Items { get; set; } = new();
}