namespace TerrariumGardenTech.Service.RequestModel.TerrariumImage;

public class TerrariumImageCreateRequest
{
    public int TerrariumId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}