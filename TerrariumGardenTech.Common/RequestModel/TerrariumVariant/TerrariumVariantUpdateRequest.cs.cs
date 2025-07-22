namespace TerrariumGardenTech.Common.RequestModel.TerrariumVariant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class TerrariumVariantUpdateRequest
{
    public int TerrariumVariantId { get; set; }

    public int TerrariumId { get; set; }

    public string VariantName { get; set; } = string.Empty;

    public decimal Price { get; set; }
    [FromForm]
    public IFormFile? ImageFile { get; set; } // Thay UrlImage bằng file upload
    public int StockQuantity { get; set; }

    public DateTime? UpdatedAt { get; set; }
}