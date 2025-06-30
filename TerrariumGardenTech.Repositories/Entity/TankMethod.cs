namespace TerrariumGardenTech.Repositories.Entity;

public class TankMethod
{
    public int TankMethodId { get; set; }
    public string TankMethodType { get; set; } = string.Empty;
    public string TankMethodDescription { get; set; } = string.Empty;
    public virtual ICollection<TerrariumTankMethod> TerrariumTankMethods { get; set; } = [];
}