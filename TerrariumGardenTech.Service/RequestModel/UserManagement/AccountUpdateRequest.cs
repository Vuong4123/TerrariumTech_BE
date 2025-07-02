using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.UserManagement
{
    public class AccountUpdateRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }  
        public string Gender { get; set; } = string.Empty;
        public int RoleId { get; set; }  // Có thể đổi role
        public string? Password { get; set; }  // Nếu muốn đổi mật khẩu
    }

}
