using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.Terrarium
{
    public sealed class TerrariumCardResponse
    {
        public int TerrariumId { get; init; }
        public string TerrariumName { get; init; }
        public string? ThumbnailUrl { get; init; }

        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public int? Stock { get; init; }
        public string Status { get; init; }

        public double AverageRating { get; init; }
        public int FeedbackCount { get; init; }
        public int PurchaseCount { get; init; }
        public string Quantitative { get; set; } // Đơn vị tính (ví dụ: "bộ", "cái", "chậu")
    }
}
