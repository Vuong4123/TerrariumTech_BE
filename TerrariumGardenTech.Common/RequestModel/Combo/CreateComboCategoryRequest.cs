namespace TerrariumGardenTech.Common.RequestModel.Combo;

// Request Models
public class CreateComboCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
}

public class UpdateComboCategoryRequest : CreateComboCategoryRequest
{
    public int ComboCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateComboRequest
{
    public int ComboCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal ComboPrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public int StockQuantity { get; set; } = 0;
    public bool IsFeatured { get; set; } = false;
    public List<ComboItemRequest> Items { get; set; } = new();
}

public class ComboItemRequest
{
    public int? TerrariumId { get; set; }
    public int? TerrariumVariantId { get; set; }
    public int? AccessoryId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateComboRequest : CreateComboRequest
{
    public int ComboId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AddComboToCartRequest
{
    public int ComboId { get; set; }
    public int Quantity { get; set; } = 1;
}
public class GetCombosRequest
{
    public int? CategoryId { get; set; }
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFeatured { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string SortBy { get; set; } = "name"; // name, price, created, sold, discount
    public string SortOrder { get; set; } = "asc"; // asc, desc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}