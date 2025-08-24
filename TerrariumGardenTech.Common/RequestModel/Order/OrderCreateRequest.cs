namespace TerrariumGardenTech.Common.RequestModel.Order;

/// <summary>Payload tạo đơn hàng</summary>
//public class OrderCreateRequest
//{
//    public int UserId { get; set; }
//    public int? VoucherId { get; set; }
//    //public decimal TotalAmount { get; set; }
//    public decimal? Deposit { get; set; }

//    //public string? PaymentStatus { get; set; }
//    //public string? ShippingStatus { get; set; }
//    // Thêm thuộc tính PaymentMethod để lưu phương thức thanh toán
//    //public string? PaymentMethod { get; set; }
//    // Danh sách mặt hàng
//    public List<OrderItemCreateRequest> Items { get; set; } = new();
//}

public class OrderCreateRequest
{
    public int? VoucherId { get; set; }
    public decimal? Deposit { get; set; }
    public int? AddressId { get; set; }
    public int? ComboId { get; set; }
    public List<OrderItemCreateRequest> Items { get; set; } = new();
    public int TotalAmount { get; set; } // Tổng số tiền đơn hàng, có thể tính toán từ Items
}
public class CancelOrderRequest
{
    public string CancelReason { get; set; }
    public string? AdditionalNotes { get; set; }
}
public class AcceptRefundRequest
{
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; } // Lý do từ chối nếu không duyệt
}

public class RefundRequestDto
{
    public int RefundId { get; set; }
    // Thông tin refund
    public int UserId { get; set; }
    public string UserEmail { get; set; }
    public int OrderId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; }
    public string RefundStatus { get; set; }
    public DateTime RequestDate { get; set; }
    public string OrderStatus { get; set; }
}