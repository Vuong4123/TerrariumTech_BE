namespace TerrariumGardenTech.Service.RequestModel.MemberShip
{
    public class CreateMembershipRequest
    {
        public string MembershipType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
