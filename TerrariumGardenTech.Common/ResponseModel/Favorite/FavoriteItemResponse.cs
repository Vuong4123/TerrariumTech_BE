using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Common.ResponseModel.Favorite
{
    public class FavoriteItemResponse
    {
        public int FavoriteId { get; set; }
        public LikeType Type { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }          // Accessory.Name / Terrarium.TerrariumName
        public decimal? Price { get; set; }       // Accessory.Price hoặc Terrarium.MinPrice
        public string ThumbnailUrl { get; set; }  // ảnh đầu tiên nếu có
        public DateTime CreatedAt { get; set; }
    }
}
