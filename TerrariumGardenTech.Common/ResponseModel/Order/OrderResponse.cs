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


    public decimal? OriginalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime? OrderDate { get; set; }

    public string Status { get; set; }


    public string PaymentStatus { get; set; } = string.Empty;
    public string TransactionId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;

    public bool IsPayFull { get; set; }
    public string Note { get; set; }
    public List<RefundResponseOrder> Refunds { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new();
}
public class RefundResponseOrder
{
    public string Status { get; set; }
    public string Reason { get; set; }
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
public class RefundResponse {
    public int OrderId { get; set; }
    public IEnumerable<int> RefundId {get; set; }
}
public class MembershipCreationResult
{
    public int MembershipId { get; set; }
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; }
    public MomoQrResponse? MomoQrResponse { get; set; } // Chỉ có khi PaymentMethod = "Momo"
    public WalletPaymentInfo? WalletPaymentInfo { get; set; } // Khi PaymentMethod = "Wallet"
}
public class WalletPaymentInfo
{
    public int WalletId { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal NewBalance { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; }
}

// Response model
public class RejectOrderResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; }
    public DateTime RejectedAt { get; set; }
    public string RejectReason { get; set; }
    public string RejectedBy { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundStatus { get; set; }
    public string Message { get; set; }
}
