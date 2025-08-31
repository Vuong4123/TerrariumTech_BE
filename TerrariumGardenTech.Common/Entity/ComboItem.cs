using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity;

public class ComboItem
{
    public int ComboItemId { get; set; }
    public int ComboId { get; set; }
    public int? TerrariumId { get; set; }
    public int? TerrariumVariantId { get; set; }
    public int? AccessoryId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int ParentOrderItemId { get; set; }

    // Navigation Properties
    public virtual Combo Combo { get; set; } = null!;

    public virtual TerrariumVariant? TerrariumVariant { get; set; }
    public virtual Accessory? Accessory { get; set; }
}