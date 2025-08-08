using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.Payment
{
    public class VnPayResultDto
    {
        public int OrderId { get; set; }
        public bool Success { get; set; }
        public int AmountVnd { get; set; }          // VND (đã chia 100)
        public string? TransactionId { get; set; }  // vnp_TransactionNo
        public string? BankCode { get; set; }
        public string? CardType { get; set; }
        public string? ResponseCode { get; set; }   // vnp_ResponseCode
        public string? PayDateRaw { get; set; }     // yyyyMMddHHmmss
    }
}
