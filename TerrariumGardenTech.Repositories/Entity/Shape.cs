using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Repositories.Entity;

public class Shape
{
    [Key] public int ShapeId { get; set; }

    public string ShapeName { get; set; } = string.Empty;
    public string ShapeDescription { get; set; } = string.Empty;
    public string ShapeMaterial { get; set; } = string.Empty;
    public virtual ICollection<Terrarium> Terrarium { get; set; } = [];
}