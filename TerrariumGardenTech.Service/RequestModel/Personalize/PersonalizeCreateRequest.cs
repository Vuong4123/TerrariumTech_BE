namespace TerrariumGardenTech.Service.RequestModel.Personalize
{
    public class PersonalizeCreateRequest
    {
        // public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;   
        public string Shape { get; set; } = string.Empty;
        public string TankMethod { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
    }
}
