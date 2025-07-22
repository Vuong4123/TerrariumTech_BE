namespace TerrariumGardenTech.Common.ResponseModel.Accessory
{
    public class AccessoryImageResponse
    {
        public int AccessoryImageId { get; set; }
        public int AccessoryId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
