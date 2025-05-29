using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.UserManagement
{
    public class AccountCreateRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }  // Mật khẩu
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int RoleId { get; set; }  // 2: Staff, 3: Manager
    }

}
