using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Payment
{
    public class DirectPaymentRequest
    {
        public int UserId { get; set; }
        public int PackageId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
