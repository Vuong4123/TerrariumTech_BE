namespace TerrariumGardenTech.Common.RequestModel.Terrarium;

public class TerrariumImageRequest
{
    public string ImageUrl { get; set; }
}

public class TerrariumCreateRequest
{
    public int EnvironmentId { get; set; }
    public int ShapeId { get; set; }
    public int TankMethodId { get; set; }
    public List<string> AccessoryNames { get; set; } = [];
    public string TerrariumName { get; set; } = default!;
    public List<string> TerrariumImages { get; set; }
    public int Stock { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Default status is "Available"
    public string bodyHTML { get; set; } = string.Empty;
    public string Quantitative { get; set; } // Đơn vị tính (ví dụ: "bộ", "cái", "chậu")
}