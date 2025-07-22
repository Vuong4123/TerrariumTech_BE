using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TerrariumGardenTech.Common.RequestModel.AccessoryImage
{
    public class AccessoryImageUploadUpdateRequest
    {
        public int AccessoryImageId { get; set; }

        public int AccessoryId { get; set; }
        [FromForm]
        public IFormFile ImageFile { get; set; }
    }
}
