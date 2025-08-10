namespace TerrariumGardenTech.Common.ResponseModel.Payment;

// TerrariumGardenTech.Service.Configs.PaymentResponseModel (hoặc namespace tương ứng)
public class PaymentResponseModel
{
    public bool Success { get; set; }
    public string PaymentMethod { get; set; }
    public string OrderDescription { get; set; }
    public string OrderId { get; set; } // Giữ string vì có thể OrderId lớn (long)
    public string PaymentId { get; set; } // vnPayTranId
    public string TransactionId { get; set; }
    public string Token { get; set; } // vnpSecureHash
    public string VnPayResponseCode { get; set; } // Mã phản hồi từ VnPay

    public decimal? Amount { get; set; } // Thêm trường này cho số tiền
    public DateTime? PaymentDate { get; set; } // Thêm trường này cho ngày thanh toán
}