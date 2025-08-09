using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Payment
{
    public class MomoQrResponse
    {
        public string PayUrl { get; set; }

        public string QrImageBase64 { get; set; }

    }
}
