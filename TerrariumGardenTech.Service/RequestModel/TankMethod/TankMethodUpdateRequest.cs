namespace TerrariumGardenTech.Service.RequestModel.TankMethod;
public class TankMethodUpdateRequest
{
    public int TankMethodId { get; set; }
    public string TankMethodType { get; set; } = string.Empty;
    public string TankMethodDescription { get; set; } = string.Empty;
}