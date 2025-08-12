namespace TerrariumGardenTech.Common.RequestModel.Payment;

public class PaymentInformationModel
{
    public int OrderId { get; set; }
    public string OrderType { get; set; }
    public string OrderDescription { get; set; }
    public string Name { get; set; }
    public bool PayAll { get; set; } = false;
    //public decimal Price { get; set; }
}
public class PaymentResponseModel
{
    public string OrderDescription { get; set; }
    public string OrderId { get; set; }
    public string PaymentMethod { get; set; }
    public string PaymentId { get; set; }
    public bool Success { get; set; }
    public string Token { get; set; }
    public string VnPayResponseCode { get; set; }
}