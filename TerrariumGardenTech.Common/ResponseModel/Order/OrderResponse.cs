using Org.BouncyCastle.Asn1.Ocsp;

using TerrariumGardenTech.Common.Enums;

using TerrariumGardenTech.Common.ResponseModel.OrderItem;

namespace TerrariumGardenTech.Common.ResponseModel.Order;

/// <summary>Dữ liệu trả về khi truy vấn đơn hàng</summary>
public class OrderResponse
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal? Deposit { get; set; }

    public DateTime? OrderDate { get; set; }


    public OrderStatus Status { get; set; } = OrderStatus.Pending;


    public string PaymentStatus { get; set; } = string.Empty;
    public string ShippingStatus { get; set; } = string.Empty;
    public string TransactionId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;

    public List<OrderItemResponse> OrderItems { get; set; } = new();
}