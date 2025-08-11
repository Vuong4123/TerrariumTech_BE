using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Common.RequestModel.MemberShip;

public class CreateMembershipPackageRequest
{
    [Required]
    public string Type { get; set; } = string.Empty; // VD: "1Month", "3Months"

    [Required] 
    public int DurationDays { get; set; } // 30, 90, 365...

    [Required]
    [Range(0, double.MaxValue)] 
    public decimal Price { get; set; } // Giá hiện tại của gói

    public string? Description { get; set; } // Mô tả thêm (tuỳ chọn)

    public bool IsActive { get; set; } = true; // Có cho phép mua không?
}