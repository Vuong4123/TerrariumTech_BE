using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Common.ResponseModel.Voucher;

public class VoucherResponse
{
    public int VoucherId { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public VoucherStatus Status { get; set; }

    // --- Bổ sung ---
    public bool IsPersonal { get; set; }
    public string? TargetUserId { get; set; }
    public int TotalUsage { get; set; }
    public int RemainingUsage { get; set; }
    public int? PerUserUsageLimit { get; set; }
}