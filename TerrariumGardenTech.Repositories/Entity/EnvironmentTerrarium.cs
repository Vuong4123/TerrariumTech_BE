using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Repositories.Entity;

public class EnvironmentTerrarium
{
    [Key] public int EnvironmentId { get; set; }

    public string EnvironmentName { get; set; } = string.Empty;
    public string EnvironmentDescription { get; set; } = string.Empty;
    public ICollection<Terrarium> Terrarium { get; set; } = [];
}