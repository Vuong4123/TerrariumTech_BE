using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.Payment
{
    public sealed class MomoIpnModel
    {
        public string partnerCode { get; set; }
        public string orderId { get; set; }        // định dạng bạn tạo: "{orderIdInternal}_{ticks}"
        public string requestId { get; set; }
        public string amount { get; set; }         // MoMo gửi dạng string
        public string orderInfo { get; set; }
        public string orderType { get; set; }
        public string transId { get; set; }
        public string resultCode { get; set; }     // "0" = success
        public string message { get; set; }
        public string payType { get; set; }
        public string responseTime { get; set; }
        public string extraData { get; set; }
        public string signature { get; set; }
    }
}
