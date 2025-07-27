using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Common.RequestModel.Order;

public class CheckoutRequest
{
    [Required]
    public string PaymentMethod { get; set; } // Phương thức thanh toán (ví dụ: "Credit Card", "PayPal", v.v.)

}