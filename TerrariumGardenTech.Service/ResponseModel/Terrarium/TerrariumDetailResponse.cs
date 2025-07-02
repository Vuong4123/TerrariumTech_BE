using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.ResponseModel.Terrarium
{
    public class TerrariumDetailResponse
    {
        public int TerrariumId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public TerrariumStatusEnum Status { get; set; }

    }
}
