using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Service.RequestModel.MemberShip;

public class UpdateMembershipRequest
{
    [Required(ErrorMessage = "PackageId không được để trống.")]
    public int PackageId { get; set; } // ID của gói membership

    [Required(ErrorMessage = "Description không được để trống.")]
    public string Description { get; set; } // Mô tả gói membership

    [Required(ErrorMessage = "Price không được để trống.")]
    public decimal Price { get; set; } // Giá gói membership

    [Required(ErrorMessage = "DurationDays không được để trống.")]
    public int DurationDays { get; set; } // Thời gian gói (ngày)
}