using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.TerrariumImage
{
    public class TerrariumImageCreateRequest
    {

        public int TerrariumId { get; set; }

        public string ImageUrl { get; set; }

        public string AltText { get; set; }

        public bool? IsPrimary { get; set; }
    }
}
