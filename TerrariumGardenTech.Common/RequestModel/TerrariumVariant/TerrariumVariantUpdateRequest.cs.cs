namespace TerrariumGardenTech.Common.RequestModel.TerrariumVariant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class TerrariumVariantUpdateRequest
{
    public int TerrariumVariantId { get; set; }

    public int TerrariumId { get; set; }

    public decimal Price { get; set; }
    public string? UrlImage { get; set; } = string.Empty;
    public int StockQuantity { get; set; }

    public List<VariantAccessoryRequest> Accessories { get; set; } = new List<VariantAccessoryRequest>();
    public DateTime? UpdatedAt { get; set; }
}