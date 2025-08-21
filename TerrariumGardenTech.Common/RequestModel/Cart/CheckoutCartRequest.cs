using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Cart
{

    public class CheckoutCartRequest
    {
        public List<int> CartItemIds { get; set; } = new();
    }
}
