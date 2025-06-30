using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.ResponseModel.Terrarium
{
    public class TerrariumDetailResponse
    {
        public int TerrariumId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public TerrariumStatusEnum Status { get; set; }

    }
}
