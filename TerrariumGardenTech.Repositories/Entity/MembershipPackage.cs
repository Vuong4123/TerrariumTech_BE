using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Repositories.Entity
{
    public class MembershipPackage
    {
        public int Id { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; // VD: "1Month", "3Months"

        [Required]
        public int DurationDays { get; set; } // 30, 90, 365...

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } // 49_000m

        public string? Description { get; set; } // Tùy chọn: mô tả cho admin/frontend

        public bool IsActive { get; set; } = true; // Cho phép tắt tạm thời

        public ICollection<Membership> Memberships { get; set; }
    }

}
