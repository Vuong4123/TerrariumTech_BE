namespace TerrariumGardenTech.Common.ResponseModel.Combo;

// Response Models
public class ComboCategoryResponse
{
    public int ComboCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int TotalCombos { get; set; }
    public int ActiveCombos { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ComboResponse
{
    public int ComboId { get; set; }
    public int ComboCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal ComboPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal SaveAmount => OriginalPrice - ComboPrice;
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int StockQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public bool IsInStock => StockQuantity > 0;
    public List<ComboItemResponse> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ComboItemResponse
{
    public int ComboItemId { get; set; }
    public int TerrariumId { get; set; }
    public int? TerrariumVariantId { get; set; }
    public int? AccessoryId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}

public class CartItemResponse
{
    public int CartId { get; set; }
    public string ItemType { get; set; } = string.Empty; // "Single" or "Combo"
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    // For single items
    public int? TerrariumVariantId { get; set; }

    public int? AccessoryId { get; set; }
    public string? SingleProductName { get; set; }
    public decimal? SingleProductPrice { get; set; }

    // For combo items
    public int? ComboId { get; set; }

    public string? ComboName { get; set; }
    public decimal? ComboPrice { get; set; }
    public decimal? ComboOriginalPrice { get; set; }
    public decimal? ComboDiscountPercent { get; set; }
    public List<ComboItemResponse>? ComboItems { get; set; }
}