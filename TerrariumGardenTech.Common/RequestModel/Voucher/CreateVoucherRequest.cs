using System.ComponentModel.DataAnnotations;
using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Common.RequestModel.Voucher;

public class CreateVoucherRequest
{
    [Required, StringLength(50)] public string Code { get; set; }
    [Required, StringLength(100)] public string Description { get; set; }

    // Cho phép amount hoặc percent (tuỳ business chọn 1 trong 2)
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    [Required] public VoucherStatus Status { get; set; }

    // --- Bổ sung theo yêu cầu ---
    public bool IsPersonal { get; set; } = false;
    public string? TargetUserId { get; set; }  // nếu IsPersonal = true
    public decimal? MinOrderAmount { get; set; }
    [Range(0, int.MaxValue)] public int TotalUsage { get; set; } = 0;      // tổng lượt cấp phát
    [Range(0, int.MaxValue)] public int? PerUserUsageLimit { get; set; }   // null => không giới hạn mỗi user
}