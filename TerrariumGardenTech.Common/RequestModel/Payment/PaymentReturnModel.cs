namespace TerrariumGardenTech.Common.RequestModel.Payment;

public class PaymentReturnModel
{
    public int OrderId { get; set; }
    // Số tiền PayOS trả (VND nguyên)
    public long Amount { get; set; }
    public PaymentReturnModelStatus StatusReturn { get; set; }
    public string? TransactionId { get; set; }
    // Một số tích hợp PayOS trả "code" = "00" khi thành công
    public string Code { get; set; }

    // Nếu PayOS trả orderCode (thay vì OrderId)
    public long? OrderCode { get; set; }
}

public enum PaymentReturnModelStatus
{
    Success,
    Fail
}