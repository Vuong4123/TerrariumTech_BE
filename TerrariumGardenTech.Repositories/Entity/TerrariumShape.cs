using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerrariumGardenTech.Repositories.Entity;

 public class TerrariumShape
{
    [Key]
    public int TerrariumShapeId { get; set; }
    public int TerrariumId { get; set; }
    public int ShapeId { get; set; }
    [ForeignKey(nameof(TerrariumId))]
    public Terrarium Terrarium { get; set; } = null!;
    [ForeignKey(nameof(ShapeId))]
    public Shape Shape { get; set; } = null!;
    
    
}