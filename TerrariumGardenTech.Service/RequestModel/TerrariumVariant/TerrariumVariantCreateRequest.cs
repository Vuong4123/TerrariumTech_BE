using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.TerrariumVariant
{
    public class TerrariumVariantCreateRequest
    {

        public int TerrariumId { get; set; }

        public string VariantName { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        public int? StockQuantity { get; set; }
    }
}
