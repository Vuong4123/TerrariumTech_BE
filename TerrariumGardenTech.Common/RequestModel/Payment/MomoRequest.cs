using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Payment
{
    public class MomoRequest
    {
        public int OrderId { get; set; }
        public string OrderInfo { get; set; }
        //public int Amount { get; set; }
        public decimal? FinalAmount { get; set; } // Số tiền FE đã tính (sau voucher/10%)
        public int? VoucherId { get; set; }
        public bool PayAll { get; set; } = false;
    }
}
