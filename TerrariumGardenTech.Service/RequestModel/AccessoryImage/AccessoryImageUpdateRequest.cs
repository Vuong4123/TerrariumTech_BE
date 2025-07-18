using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.AccessoryImage
{
    public class AccessoryImageUpdateRequest
    {
        public int AccessoryImageId { get; set; }

        public int AccessoryId { get; set; }

        public string ImageUrl { get; set; }

    }
}
