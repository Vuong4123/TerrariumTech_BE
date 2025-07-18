namespace TerrariumGardenTech.Service.ResponseModel.Terrarium;

public class TerrariumDetailResponse
{
    public int TerrariumId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; }
    public List<string> Environments { get; set; } = [];
    public List<string> Shapes { get; set; } = [];
    public List<string> TankMethods { get; set; } = [];
    public List<TerrariumImageResponse> TerrariumImages { get; set; } = [];
}