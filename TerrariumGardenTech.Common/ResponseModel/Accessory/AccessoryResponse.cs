namespace TerrariumGardenTech.Common.ResponseModel.Accessory;

public class AccessoryResponse
{
    public int AccessoryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int? StockQuantity { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Status { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int FeedbackCount { get; set; }
    // ✅ mới
    public int PurchaseCount { get; set; }
    public string Quantitative { get; set; } // Đơn vị tính (ví dụ: "bộ", "cái", "chậu")
    public List<AccessoryImageResponse> AccessoryImages { get; set; } = [];
}