using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Service.RequestModel.MemberShip
{
    public class CreateMembershipForUserRequest
    {
        [Required(ErrorMessage = "UserId là bắt buộc.")]
        public int UserId { get; set; } // ID của người dùng

        [Required(ErrorMessage = "PackageId là bắt buộc.")]
        public int PackageId { get; set; } // ID của gói membership

        [Required(ErrorMessage = "StartDate là bắt buộc.")]
        public DateTime StartDate { get; set; } // Ngày bắt đầu membership

        [Required(ErrorMessage = "Price là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số dương.")]
        public decimal Price { get; set; } // Giá membership

        [Required(ErrorMessage = "DurationDays là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời gian phải lớn hơn 0.")]
        public int DurationDays { get; set; } // Số ngày của gói membership (thời gian hiệu lực)

        public string Description { get; set; } // Mô tả của gói membership
    }
}