namespace TerrariumGardenTech.Common.ResponseModel.Personalize;

public class PersonalizeResponse
{
    public int PersonalizeId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Shape { get; set; } = string.Empty;
    public string TankMethod { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string size { get; set; } = string.Empty;
}