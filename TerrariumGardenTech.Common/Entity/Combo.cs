using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity;

public class Combo
{
    public int ComboId { get; set; }
    public int ComboCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal ComboPrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public int StockQuantity { get; set; } = 0;
    public int SoldQuantity { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual ComboCategory ComboCategory { get; set; } = null!;

    public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}