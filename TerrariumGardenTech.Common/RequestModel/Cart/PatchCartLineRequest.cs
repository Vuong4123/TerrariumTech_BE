using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Cart
{
    public class PatchCartLineRequest
    {
        public int? Quantity { get; set; }               // thay số lượng
        public int? TerrariumId { get; set; }            // đổi loại bể
        public int? VariantId { get; set; }              // đổi variant (SKU)
    }
}
