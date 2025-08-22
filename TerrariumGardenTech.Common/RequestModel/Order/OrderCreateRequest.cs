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