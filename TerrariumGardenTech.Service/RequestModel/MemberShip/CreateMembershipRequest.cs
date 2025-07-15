using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Service.RequestModel.MemberShip
{
    public class CreateMembershipRequest
    {
        [Required]
        public string MembershipType { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }
    }
}
