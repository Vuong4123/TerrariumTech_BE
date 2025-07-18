using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.Service.ResponseModel.Voucher;

public class VoucherResponse
{
    public int VoucherId { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public VoucherStatus Status { get; set; } // Trả về status dưới dạng enum
}