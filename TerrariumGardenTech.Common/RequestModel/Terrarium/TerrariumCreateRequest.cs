namespace TerrariumGardenTech.Common.RequestModel.Terrarium;

public class TerrariumCreateRequest
{
    public int EnvironmentId { get; set; }
    public int ShapeId { get; set; }
    public int TankMethodId { get; set; }
    public List<string> AccessoryNames { get; set; } = [];
    public string TerrariumName { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public decimal? MinPrice { get; set; }  // Giá thấp nhất
    public decimal? MaxPrice { get; set; }  // Giá cao nhất
    public int Stock { get; set; }
    public string Status { get; set; }
    public string bodyHTML { get; set; } = string.Empty;
}