namespace TerrariumGardenTech.Common.RequestModel.Terrarium;

public class TerrariumCreateRequest
{
    public int EnvironmentId { get; set; }
    public int ShapeId { get; set; }
    public int TankMethodId { get; set; }
    public List<string> AccessoryNames { get; set; } = [];
    public string TerrariumName { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Default status is "Available"
    public string bodyHTML { get; set; } = string.Empty;
}