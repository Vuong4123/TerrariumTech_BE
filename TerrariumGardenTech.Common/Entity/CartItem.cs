using System.Text.Json.Serialization;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity;

/// <summary>
/// Entity lưu trữ sản phẩm trong giỏ hàng
/// Hỗ trợ cả sản phẩm đơn lẻ và bundle
/// </summary>
public class CartItem
{
    public int CartItemId { get; set; }
    public int CartId { get; set; }

    [JsonIgnore]
    public Cart Cart { get; set; }

    // === THÔNG TIN SẢN PHẨM ===
    public int? AccessoryId { get; set; }
    public Accessory Accessory { get; set; }
    public int? AccessoryQuantity { get; set; }

    public int? TerrariumVariantId { get; set; }
    public TerrariumVariant TerrariumVariant { get; set; }
    public int? TerrariumVariantQuantity { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // === BUNDLE MANAGEMENT ===
    /// <summary>
    /// ID của item cha (bể thủy sinh chính)
    /// - null: là sản phẩm độc lập hoặc sản phẩm chính
    /// - có giá trị: là phụ kiện thuộc bundle của item cha
    /// </summary>
    public int? ParentCartItemId { get; set; }

    /// <summary>
    /// Navigation property đến item cha
    /// </summary>
    [JsonIgnore]
    public CartItem? ParentCartItem { get; set; }

    /// <summary>
    /// Danh sách phụ kiện con (nếu item này là bể chính)
    /// </summary>
    [JsonIgnore]
    public ICollection<CartItem> ChildItems { get; set; } = new List<CartItem>();

    /// <summary>
    /// Loại sản phẩm:
    /// - "SINGLE": Sản phẩm mua riêng lẻ
    /// - "MAIN_ITEM": Bể thủy sinh chính (có thể có phụ kiện kèm theo)
    /// - "BUNDLE_ACCESSORY": Phụ kiện thuộc combo với bể
    /// </summary>
    public string ItemType { get; set; } = CommonData.CartItemType.SINGLE;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
