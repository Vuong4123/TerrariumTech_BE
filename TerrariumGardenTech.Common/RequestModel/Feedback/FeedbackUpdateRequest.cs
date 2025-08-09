using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Feedback
{
    public class FeedbackUpdateRequest
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
        
    }
}
