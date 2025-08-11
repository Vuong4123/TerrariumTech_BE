using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }

        public int? AccessoryId { get; set; }
        public int? TerrariumId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; }
        public virtual Accessory Accessory { get; set; }
        public virtual Terrarium Terrarium { get; set; }
    }
}
