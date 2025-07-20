using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.TerrariumImage
{
    public class TerrariumImageUploadUpdateRequest
    {
        public int TerrariumImageId { get; set; }
        public int TerrariumId { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
