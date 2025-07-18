using TerrariumGardenTech.Service.ResponseModel.OrderItem;

namespace TerrariumGardenTech.Service.ResponseModel.Order;

/// <summary>Dữ liệu trả về khi truy vấn đơn hàng</summary>
public class OrderResponse
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal? Deposit { get; set; }

    public DateTime? OrderDate { get; set; }

    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string ShippingStatus { get; set; } = string.Empty;

    public List<OrderItemSummaryResponse> Items { get; set; } = new();
}