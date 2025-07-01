using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.Firebase
{
    public class CreateTokenRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string FcmToken { get; set; } = string.Empty;
    }
}
