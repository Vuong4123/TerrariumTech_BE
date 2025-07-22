using System.ComponentModel.DataAnnotations;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity;

public class EnvironmentTerrarium
{
    [Key] public int EnvironmentId { get; set; }

    public string EnvironmentName { get; set; } = string.Empty;
    public string EnvironmentDescription { get; set; } = string.Empty;
    public ICollection<Terrarium> Terrarium { get; set; } = [];
}