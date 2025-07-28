using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.Feedback
{
    public class FeedbackResponse
    {
        public int FeedbackId { get; set; }
        public int OrderItemId { get; set; }
        public int? Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public IEnumerable<string>? Images { get; set; }
    }
}
