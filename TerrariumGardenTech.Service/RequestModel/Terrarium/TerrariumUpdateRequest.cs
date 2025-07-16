using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.RequestModel.Terrarium
{
    public class TerrariumUpdateRequest
    {
        public int TerrariumId { get; set; }

        public string TankMethodType { get; set; } = string.Empty; 
        public string Shape { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public List<string> AccessoryNames { get; set; } = [];
        public string TerrariumName { get; set; } = default!;
        public string Description { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        public int Stock { get; set; }

        public string Status { get; set; }

        public string? bodyHTML { get; set; } = string.Empty;
    }
}
