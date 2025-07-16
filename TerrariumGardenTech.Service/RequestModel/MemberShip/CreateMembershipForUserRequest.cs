using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.MemberShip
{
    public class CreateMembershipForUserRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string MembershipType { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }
    }

}
