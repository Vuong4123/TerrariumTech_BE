using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerrariumGardenTech.Repositories.Entity;

public class TerrariumAccessory
{
    [Key]
    public int TerrariumAccessoryId { get; set; }
    public int TerrariumId { get; set; }
    public int AccessoryId { get; set; }
    [ForeignKey(nameof(TerrariumId))]
    public Terrarium Terrarium { get; set; } = null!;
    [ForeignKey(nameof(AccessoryId))]
    public Accessory Accessory { get; set; } = null!;
}
