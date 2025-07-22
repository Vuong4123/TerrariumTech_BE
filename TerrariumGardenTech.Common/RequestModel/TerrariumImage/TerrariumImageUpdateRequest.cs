namespace TerrariumGardenTech.Common.RequestModel.TerrariumImage;

public class TerrariumImageUpdateRequest
{
    public int TerrariumImageId { get; set; }
    public int TerrariumId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}