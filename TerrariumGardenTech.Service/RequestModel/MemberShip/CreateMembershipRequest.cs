namespace TerrariumGardenTech.Service.RequestModel.MemberShip
{
    public class CreateMembershipRequest
    {
        public int UserId { get; set; }
        public string MembershipType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }
}
