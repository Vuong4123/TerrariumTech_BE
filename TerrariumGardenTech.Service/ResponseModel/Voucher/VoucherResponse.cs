using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.ResponseModel.Voucher
{
    public class VoucherResponse
    {
        public int VoucherId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime ValidFrom { get; set; }  // Ensure this is DateTime
        public DateTime ValidTo { get; set; }    // Ensure this is DateTime
        public string Status { get; set; }
    }


}
