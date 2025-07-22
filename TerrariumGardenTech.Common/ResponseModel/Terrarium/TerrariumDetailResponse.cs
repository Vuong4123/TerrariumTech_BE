namespace TerrariumGardenTech.Common.ResponseModel.Terrarium;

public class TerrariumDetailResponse
{
    public int TerrariumId { get; set; }
    public int EnvironmentId { get; set; }
    public int ShapeId { get; set; }
    public int TankMethodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; }
    public List<TerrariumImageResponse> TerrariumImages { get; set; } = [];
}