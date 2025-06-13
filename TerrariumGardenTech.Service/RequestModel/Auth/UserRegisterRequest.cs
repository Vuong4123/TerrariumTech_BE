using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.Auth
{
    public class UserRegisterRequest
    {
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; }
        public string FullName { get; set; }
    }
}
