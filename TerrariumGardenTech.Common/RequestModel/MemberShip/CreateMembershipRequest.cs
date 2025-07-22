using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Common.RequestModel.MemberShip;

public class CreateMembershipRequest
{
    [Required] public int UserId { get; set; } // Đảm bảo kiểu int

    [Required] public int PackageId { get; set; } // Đảm bảo kiểu int

    [Required] public DateTime StartDate { get; set; }
}