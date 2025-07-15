using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Repositories.Entity
{
    public class AccessoryShape
    {
        [Key]
        public int AccessoryShapeId { get; set; }
        public int AccessoryId { get; set; }
        public int ShapeId { get; set; }
        [ForeignKey(nameof(AccessoryId))]
        public Accessory Accessory { get; set; } = null!;
        [ForeignKey(nameof(ShapeId))]
        public Shape Shape { get; set; } = null!;
    }
}
