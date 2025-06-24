using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.RequestModel.Personalize
{
    public class PersonalizeCreateRequest
    {
        public int PersonalizeId { get; set; }

        public int UserId { get; set; }

        public string Preferences { get; set; }

        public string Theme { get; set; }

        public string Language { get; set; }
    }
}
