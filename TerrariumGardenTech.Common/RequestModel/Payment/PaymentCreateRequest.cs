namespace TerrariumGardenTech.Common.RequestModel.Payment;

public class PaymentCreateRequest
{
    public int OrderId { get; set; }
    public string Description { get; set; }
}