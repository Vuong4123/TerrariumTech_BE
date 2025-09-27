namespace TerrariumGardenTech.Common.Entity;

public class PaymentConfig
{
    public int Id { get; set; }
    public decimal DepositPercent { get; set; }
    public decimal FullPaymentDiscountPercent { get; set; }
    public decimal FreeshipAmount { get; set; }
    public decimal OrderAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
