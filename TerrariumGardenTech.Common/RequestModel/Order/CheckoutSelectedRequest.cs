using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Order
{
    public record CheckoutSelectedRequest(List<int> CartItemIds);
}
