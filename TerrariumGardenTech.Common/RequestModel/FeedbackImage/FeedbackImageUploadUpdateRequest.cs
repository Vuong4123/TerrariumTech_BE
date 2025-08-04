using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.FeedbackImage
{
    public class FeedbackImageUploadUpdateRequest
    {
        public int FeedbackImageId { get; set; }
        public int FeedbackId { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
