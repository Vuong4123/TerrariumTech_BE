using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.ResponseModel.Terrarium
{
    public class TerrariumImageResponse
    {
        public int TerrariumImageId { get; set; }
        public int TerrariumId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
