using System.ComponentModel.DataAnnotations;
using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Common.RequestModel.Voucher;

public class UpdateVoucherRequest
{
    [Required] public int VoucherId { get; set; }
    [Required, StringLength(50)] public string Code { get; set; }
    [Required, StringLength(100)] public string Description { get; set; }

    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    [Required] public VoucherStatus Status { get; set; }

    // --- Bổ sung theo yêu cầu ---
    public bool IsPersonal { get; set; } = false;
    public string? TargetUserId { get; set; }

    [Range(0, int.MaxValue)] public int TotalUsage { get; set; } = 0;
    [Range(0, int.MaxValue)] public int RemainingUsage { get; set; } = 0;  // cho phép chỉnh tay khi update
    public int? PerUserUsageLimit { get; set; }
}