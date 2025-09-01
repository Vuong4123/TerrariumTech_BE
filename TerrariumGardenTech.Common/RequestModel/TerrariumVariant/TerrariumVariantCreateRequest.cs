using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TerrariumGardenTech.Common.RequestModel.TerrariumVariant;

public class TerrariumVariantCreateRequest
{
    public int TerrariumId { get; set; }

    public string VariantName { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public string? UrlImage { get; set; } = string.Empty;
    public int StockQuantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public List<VariantAccessoryRequest> Accessories { get; set; } = new List<VariantAccessoryRequest>();
}
public class VariantAccessoryRequest
{
    public int AccessoryId { get; set; }
    public int Quantity { get; set; } = 1;
}