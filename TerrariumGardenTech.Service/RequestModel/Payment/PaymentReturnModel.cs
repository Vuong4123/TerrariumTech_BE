namespace TerrariumGardenTech.Service.RequestModel.Payment;

public class PaymentReturnModel
{
    public int OrderId { get; set; }
    public PaymentReturnModelStatus StatusReturn { get; set; }
    public string? Status { get; set; }
}

public enum PaymentReturnModelStatus
{
    Success,
    Fail
}