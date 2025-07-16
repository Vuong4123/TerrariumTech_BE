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
        public int UserId { get; set; }
        public int PackageId { get; set; } 
        public DateTime StartDate { get; set; }
    }

}
