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
        public DateTime UpdatedAt { get; set; }
        //public IEnumerable<string>? Images { get; set; }
        public List<FeedbackImageResponse> Images { get; set; } = new();

        // NEW:
        public int? TerrariumId { get; set; }
        public string? TerrariumName { get; set; }
        public int? AccessoryId { get; set; }
        public string? AccessoryName { get; set; }
    }
}
