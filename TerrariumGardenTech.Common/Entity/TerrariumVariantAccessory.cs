using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity
{
    public class TerrariumVariantAccessory
    {
        [Key] public int TerrariumVariantAccessoryId { get; set; }

        public int TerrariumVariantId { get; set; }
        public int AccessoryId { get; set; }
        public int Quantity { get; set; } = 1; // Số lượng accessory cần cho variant này

        [ForeignKey(nameof(TerrariumVariantId))]
        public TerrariumVariant TerrariumVariant { get; set; } = null!;

        [ForeignKey(nameof(AccessoryId))]
        public Accessory Accessory { get; set; } = null!;
    }
}
