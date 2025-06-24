using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.Service.RequestModel.MemberShip
{
    public class CreateMembershipRequest
    {
        public int UserId { get; set; }
        public MembetShipType MembershipType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
