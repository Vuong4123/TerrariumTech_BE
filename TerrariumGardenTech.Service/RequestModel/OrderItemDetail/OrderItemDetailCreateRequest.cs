using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.OrderItemDetail
{
    public class OrderItemDetailCreateRequest
    {
        public int OrderItemId { get; set; }
        public string DetailKey { get; set; }
        public string DetailValue { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
