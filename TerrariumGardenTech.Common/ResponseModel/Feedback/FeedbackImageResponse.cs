using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.Feedback
{
    public class FeedbackImageResponse
    {
        public int FeedbackId { get; set; }
        public int FeedbackImageId { get; set; }
        public string Url { get; set; }
    }
}
