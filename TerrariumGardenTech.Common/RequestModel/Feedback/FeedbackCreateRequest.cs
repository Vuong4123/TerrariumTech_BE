using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Feedback
{
    public class FeedbackCreateRequest
    {
        public int OrderItemId { get; set; }
        public int Rating { get; set; } // 1–5
        public string? Comment { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
}
