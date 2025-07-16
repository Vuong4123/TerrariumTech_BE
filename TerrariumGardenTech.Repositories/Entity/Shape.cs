using System.ComponentModel.DataAnnotations;
namespace TerrariumGardenTech.Repositories.Entity;

public class Shape
{
    [Key]
    public int ShapeId { get; set; }
    public string ShapeName { get; set; } = string.Empty;
    public string ShapeDescription { get; set; }    = string.Empty;
    public string ShapeSize { get; set; } = string.Empty;
    public int ShapeHeight { get; set; }
    public int ShapeWidth { get; set; }
    public int ShapeLength { get; set; }
    public float ShapeVolume { get; set; }
    public string ShapeMaterial { get; set; } = string.Empty;
    

    public virtual ICollection<TerrariumShape> TerrariumShapes { get; set; } = [];


}