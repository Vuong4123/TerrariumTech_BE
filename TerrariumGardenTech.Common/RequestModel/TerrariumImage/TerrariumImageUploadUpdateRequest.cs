using Microsoft.AspNetCore.Http;

namespace TerrariumGardenTech.Common.RequestModel.TerrariumImage
{
    public class TerrariumImageUploadUpdateRequest
    {
        public int TerrariumImageId { get; set; }
        public int TerrariumId { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
