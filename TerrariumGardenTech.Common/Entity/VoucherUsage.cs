using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace TerrariumGardenTech.Repositories.Entity
{
    [Table("VoucherUsage")]
    [PrimaryKey(nameof(VoucherId), nameof(UserId))] 
    public class VoucherUsage
    {
        public int VoucherId { get; set; }

        [MaxLength(64)]
        public string UserId { get; set; } = null!;

        public int UsedCount { get; set; } = 0;

        // navigation để EF “bò” từ Voucher sang VoucherUsage
        [ForeignKey(nameof(VoucherId))]
        public virtual Voucher Voucher { get; set; } = null!;
    }
}
