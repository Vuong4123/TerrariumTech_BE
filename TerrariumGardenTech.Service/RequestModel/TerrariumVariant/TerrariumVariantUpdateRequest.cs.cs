using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.TerrariumVariant
{
    public class TerrariumVariantUpdateRequest
    {
        public int TerrariumVariantId { get; set; }

        public int TerrariumId { get; set; }

        public string VariantName { get; set; } = string.Empty;

        public decimal? AdditionalPrice { get; set; }

        public int? StockQuantity { get; set; }
    }
}
