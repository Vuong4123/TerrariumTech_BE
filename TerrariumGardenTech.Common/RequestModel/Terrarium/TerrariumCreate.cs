using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Terrarium
{
    public class TerrariumCreate
    {
        public int EnvironmentId { get; set; }
        public int ShapeId { get; set; }
        public int TankMethodId { get; set; }
        public List<string> AccessoryNames { get; set; } = [];
        public string TerrariumName { get; set; } = default!;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        public string? bodyHTML { get; set; } = string.Empty;
    }
}
