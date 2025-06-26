using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.Personalize
{
    public class PersonalizeUpdateRequest
    {
        public int PersonalizeId { get; set; }

        public int UserId { get; set; }
        public string Type { get; set; }
        public string Shape { get; set; }
        public string TankMethod { get; set; }
        public string Theme { get; set; }
        public string Size { get; set; }
    }
}
