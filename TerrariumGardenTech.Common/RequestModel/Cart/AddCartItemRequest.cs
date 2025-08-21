using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Common.RequestModel.Cart;

/// <summary>
/// Request để thêm sản phẩm vào giỏ hàng
/// Hỗ trợ 3 trường hợp:
/// 1. Mua bể đơn lẻ
/// 2. Mua phụ kiện đơn lẻ  
/// 3. Mua bể + combo phụ kiện
/// </summary>
public class AddCartItemRequest
{
    public int TerrariumId { get; set; }
    /// <summary>
    /// ID phụ kiện (nullable - dùng khi mua phụ kiện đơn lẻ)
    /// </summary>
    public int? AccessoryId { get; set; }

    /// <summary>
    /// ID bể thủy sinh (nullable - dùng khi mua bể)
    /// </summary>
    public int? TerrariumVariantId { get; set; }

    /// <summary>
    /// Số lượng phụ kiện (chỉ có giá trị khi AccessoryId có giá trị)
    /// </summary>
    public int? AccessoryQuantity { get; set; }

    /// <summary>
    /// Số lượng bể thủy sinh (chỉ có giá trị khi TerrariumVariantId có giá trị)
    /// </summary>
    public int? VariantQuantity { get; set; }
}

/// <summary>
/// Request để thêm sản phẩm vào giỏ hàng
/// Hỗ trợ 3 trường hợp:
/// 1. Mua bể đơn lẻ
/// 2. Mua phụ kiện đơn lẻ  
/// 3. Mua bể + combo phụ kiện
/// </summary>
public class AddCartItemMultipleRequest
{
    public int TerrariumId { get; set; }
    public int TotalPrice { get; set; }
    /// <summary>
    /// Danh sách phụ kiện kèm theo bể (chỉ có giá trị khi mua bể)
    /// Frontend có thể gửi empty array [] nếu chỉ mua bể đơn lẻ
    /// </summary>
    public List<BundleAccessoryRequest>? BundleAccessories { get; set; }
}

/// <summary>
/// Chi tiết phụ kiện trong combo
/// </summary>
public class BundleAccessoryRequest
{
    /// <summary>
    /// ID phụ kiện muốn thêm vào combo
    /// </summary>
    public int AccessoryId { get; set; }

    /// <summary>
    /// Số lượng phụ kiện (phải > 0)
    /// </summary>
    public int Quantity { get; set; }
}

