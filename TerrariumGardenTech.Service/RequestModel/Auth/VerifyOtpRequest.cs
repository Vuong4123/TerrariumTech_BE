using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.Auth
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }  
    }
}
