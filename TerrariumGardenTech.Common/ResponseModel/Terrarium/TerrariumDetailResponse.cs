namespace TerrariumGardenTech.Common.ResponseModel.Terrarium;

public class TerrariumDetailResponse
{
    public int TerrariumId { get; set; }
    public int EnvironmentId { get; set; }
    public int ShapeId { get; set; }
    public int TankMethodId { get; set; }
    public string TerrariumName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? MinPrice { get; set; }  // Giá thấp nhất
    public decimal? MaxPrice { get; set; }  // Giá cao nhất
    public int Stock { get; set; }
    public string Status { get; set; }
    public List<TerrariumImageResponse> TerrariumImages { get; set; } = [];
}