using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Common.RequestModel.Transports
{
    public class UpdateTransportModel
    {
        public int TransportId { get; set; }
        public TransportStatusEnum Status { get; set; }
        public int? ContactFailNumber { get; set; }
        public int? AssignToUserId { get; set; }
        public string? Reason { get; set; }
        public IFormFile? Image { get; set; }
    }
}
