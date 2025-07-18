using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Service.RequestModel.Order;

public class CheckoutRequest
{
    [Required]
    public string PaymentMethod { get; set; } // Phương thức thanh toán (ví dụ: "Credit Card", "PayPal", v.v.)

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Số tiền thanh toán không hợp lệ.")]
    public decimal PaidAmount { get; set; } // Số tiền thanh toán
}