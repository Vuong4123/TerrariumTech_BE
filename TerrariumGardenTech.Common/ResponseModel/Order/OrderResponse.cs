using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.ResponseModel.Address;
using VNPAY.NET.Models;

namespace TerrariumGardenTech.Common.ResponseModel.Order;

/// <summary>Dữ liệu trả về khi truy vấn đơn hàng</summary>
public class OrderResponse
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal? Deposit { get; set; }


    public decimal? DiscountAmount { get; set; }
    public DateTime? OrderDate { get; set; }


    public OrderStatusEnum Status { get; set; }


    public string PaymentStatus { get; set; } = string.Empty;
    public string ShippingStatus { get; set; } = string.Empty;
    public string TransactionId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public AddressResponse Address { get; set; }
    public List<PaymentResponse> Payment { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new();
}