using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.ResponseModel.Accessory
{
    public class AccessoryImageResponse
    {
        public int AccessoryImageId { get; set; }
        public int AccessoryId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
