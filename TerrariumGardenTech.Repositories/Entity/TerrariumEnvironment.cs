using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerrariumGardenTech.Repositories.Entity;

public class TerrariumEnvironment
{
    [Key]
    public int TerrariumEnvironmentId { get; set; }
    public int TerrariumId { get; set; }
    public int EnvironmentId { get; set; }
    [ForeignKey(nameof(TerrariumId))]
    public Terrarium Terrarium { get; set; } = null!;
    [ForeignKey(nameof(EnvironmentId))]
    public EnvironmentTerrarium EnvironmentTerrarium { get; set; } = null!;
    
    
}