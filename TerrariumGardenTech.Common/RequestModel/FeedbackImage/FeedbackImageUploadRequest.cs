using Microsoft.AspNetCore.Http;

namespace TerrariumGardenTech.Common.RequestModel.FeedbackImage
{
    public class FeedbackImageUploadRequest
    {
        public int FeedbackId { get; set; }
        public IFormFile ImageFile { get; set; } // Assuming you are using IFormFile for file uploads   
    }
}
