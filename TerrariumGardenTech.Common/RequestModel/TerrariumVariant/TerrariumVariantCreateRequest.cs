namespace TerrariumGardenTech.Common.RequestModel.TerrariumVariant;

public class TerrariumVariantCreateRequest
{
    public int TerrariumId { get; set; }

    public string VariantName { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public int? StockQuantity { get; set; }
}