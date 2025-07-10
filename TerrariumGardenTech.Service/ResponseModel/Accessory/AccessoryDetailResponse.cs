namespace TerrariumGardenTech.Service.ResponseModel.Accessory
{
    public class AccessoryDetailResponse
    {
        public int AccessoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}
