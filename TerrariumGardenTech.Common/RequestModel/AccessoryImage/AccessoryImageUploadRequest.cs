using Microsoft.AspNetCore.Http;

namespace TerrariumGardenTech.Common.RequestModel.AccessoryImage
{
    public class AccessoryImageUploadRequest
    {
        public int AccessoryId { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
