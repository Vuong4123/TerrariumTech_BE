namespace TerrariumGardenTech.Service.ResponseModel.Terrarium;

public class TerrariumImageResponse
{
    public int TerrariumImageId { get; set; }
    public int TerrariumId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}