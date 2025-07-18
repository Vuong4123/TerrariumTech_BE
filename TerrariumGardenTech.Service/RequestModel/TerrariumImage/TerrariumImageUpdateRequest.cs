namespace TerrariumGardenTech.Service.RequestModel.TerrariumImage;

public class TerrariumImageUpdateRequest
{
    public int TerrariumImageId { get; set; }

    public int TerrariumId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public bool? IsPrimary { get; set; }
}