namespace TerrariumGardenTech.Service.RequestModel.Personalize
{
    public class PersonalizeCreateRequest
    {
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Shape { get; set; }
        public string TankMethod { get; set; }
        public string Theme { get; set; }
        public string Size { get; set; }
    }
}
