using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TerrariumGardenTech.Repositories.Entity;

public class TerrariumTankMethod
{
    [Key]
    public int TerrariumTankMethodId { get; set; }
    public int TerrariumId { get; set; }
    public int TankMethodId { get; set; }
    [ForeignKey(nameof(TerrariumId))]
    public Terrarium Terrarium { get; set; } = null!;
    [ForeignKey(nameof(TankMethodId))]
    public TankMethod TankMethod { get; set; } = null!;
}