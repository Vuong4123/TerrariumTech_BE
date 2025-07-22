using System.ComponentModel.DataAnnotations;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity;

public class Shape
{
    [Key] public int ShapeId { get; set; }

    public string ShapeName { get; set; } = string.Empty;
    public string ShapeDescription { get; set; } = string.Empty;
    public string ShapeMaterial { get; set; } = string.Empty;
    public virtual ICollection<Terrarium> Terrarium { get; set; } = [];
}