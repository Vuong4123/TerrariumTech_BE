using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.AccessoryImage
{
    public class AccessoryImageUploadRequest
    {
        public int AccessoryId { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
