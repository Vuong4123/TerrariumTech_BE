namespace TerrariumGardenTech.Service.RequestModel.AccessoryImage;

public class AccessoryImageUpdateRequest
{
    public int AccessoryImageId { get; set; }

    public int AccessoryId { get; set; }

    public string ImageUrl { get; set; }

    public string AltText { get; set; }

    public bool? IsPrimary { get; set; }
}