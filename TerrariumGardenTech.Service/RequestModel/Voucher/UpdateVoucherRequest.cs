using System.ComponentModel.DataAnnotations;
using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.Service.RequestModel.Voucher;

public class UpdateVoucherRequest
{
    [Required] public int VoucherId { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Mã voucher không được dài quá 50 ký tự.")]
    public string Code { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Mô tả voucher không được dài quá 100 ký tự.")]
    public string Description { get; set; }

    [Required] public decimal DiscountAmount { get; set; }

    [Required] public DateTime ValidFrom { get; set; }

    [Required] public DateTime ValidTo { get; set; }

    [Required] public VoucherStatus Status { get; set; } // Sử dụng VoucherStatus enum thay vì chuỗi
}