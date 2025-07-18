namespace TerrariumGardenTech.Service.RequestModel.Payment;

public class PaymentReturnModel
{
    public int OrderId { get; set; }
    public PaymentReturnModelStatus Status { get; set; }
}

public enum PaymentReturnModelStatus
{
    Success,
    Fail,
}