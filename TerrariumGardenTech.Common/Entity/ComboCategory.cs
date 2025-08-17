namespace TerrariumGardenTech.Common.Entity;

public class ComboCategory
{
    public int ComboCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual ICollection<Combo> Combos { get; set; } = new List<Combo>();
}