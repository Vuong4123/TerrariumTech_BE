namespace TerrariumGardenTech.Common.ResponseModel.TerrariumVariant;

public class TerrariumVariantResponse
{
    public int TerrariumVariantId { get; set; }
    public int TerrariumId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? UrlImage { get; set; }
    public int StockQuantity { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<VariantAccessoryResponse> Accessories { get; set; } = new List<VariantAccessoryResponse>();
}

public class VariantAccessoryResponse
{
    public int AccessoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class TerrariumVariantCreateResponse
{
    public int TerrariumVariantId { get; set; }
    public int TerrariumId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? UrlImage { get; set; }
    public int StockQuantity { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<VariantAccessoryResponse> Accessories { get; set; } = new List<VariantAccessoryResponse>();
}

public class TerrariumVariantUpdateResponse
{
    public int TerrariumVariantId { get; set; }
    public int TerrariumId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? UrlImage { get; set; }
    public int StockQuantity { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<VariantAccessoryResponse> Accessories { get; set; } = new List<VariantAccessoryResponse>();
}