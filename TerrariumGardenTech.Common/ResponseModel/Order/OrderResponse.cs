using Org.BouncyCastle.Asn1.Ocsp;

using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel.OrderItem;
using static TerrariumGardenTech.Common.Enums.CommonData;

namespace TerrariumGardenTech.Common.ResponseModel.Order;

/// <summary>Dữ liệu trả về khi truy vấn đơn hàng</summary>
public class OrderResponse
{
    public int OrderId { get; set; }
    public int UserId { get; set; }

    public int? AddressId { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal? Deposit { get; set; }


    public decimal? DiscountAmount { get; set; }
    public DateTime? OrderDate { get; set; }


    public string Status { get; set; }


    public string PaymentStatus { get; set; } = string.Empty;
    public string TransactionId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;

    public List<OrderItemResponse> OrderItems { get; set; } = new();
}
public class CancelOrderResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; }
    public DateTime CancelledAt { get; set; }
    public string CancelReason { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundStatus { get; set; }
    public string Message { get; set; }
}
public class AcceptRefundResponse
{
    public int RefundId { get; set; }
    public int OrderId { get; set; }
    public string RefundStatus { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public int ProcessedBy { get; set; }
    public bool IsApproved { get; set; }
    public string Message { get; set; }
}
public class MembershipCreationResult
{
    public int MembershipId { get; set; }
    public int OrderId { get; set; }
    public MomoQrResponse MomoQrResponse { get; set; } // URL để người dùng thanh toán
}