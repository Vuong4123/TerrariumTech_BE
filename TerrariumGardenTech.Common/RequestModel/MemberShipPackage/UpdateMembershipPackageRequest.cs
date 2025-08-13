using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.MemberShipPackage
{
    public class UpdateMembershipPackageRequest
    {
        public int PackageId { get; set; }
        public string? Type { get; set; }          // null => giữ nguyên
        public int? DurationDays { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }        // “status” của gói
    }
}
