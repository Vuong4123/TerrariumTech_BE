using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.Terrarium
{
    public class TerrariumCreateRequest
    {

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal? Price { get; set; }

        public int Stock { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        public string Shape { get; set; }

        public string TankMethod { get; set; }

        public string Theme { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int? AccessoryId { get; set; }
        public string Size { get; set; }
        public string bodyHTML { get; set; } 
    }
}
