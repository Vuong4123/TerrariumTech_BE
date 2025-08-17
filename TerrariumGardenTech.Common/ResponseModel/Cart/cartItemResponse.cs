namespace TerrariumGardenTech.Common.ResponseModel.Cart
{
    public class CartItemOptionDto    // option để user đổi nhanh trên dropdown
    {
        public int VariantId { get; set; }
        public string Label { get; set; } = default!;
        public decimal Price { get; set; }
    }

    public class TerrariumLiteDto     // thông tin bể + danh sách variant/option
    {
        public int TerrariumId { get; set; }
        public string Name { get; set; } = default!;
        public string CoverImage { get; set; } = default!;
        public List<CartItemOptionDto> Options { get; set; } = new();
    }

    public class VariantLiteDto       // thông tin variant hiện chọn
    {
        public int VariantId { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = default!;
    }

    public class CartItemDetail
    {
        public string ProductName { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartItemResponse
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }

        // giữ lại field cũ để tương thích
        public int? AccessoryId { get; set; }
        public int? TerrariumVariantId { get; set; }

        // ✅ phần hydrate thêm
        public TerrariumLiteDto? Terrarium { get; set; }
        public VariantLiteDto? Variant { get; set; }

        public List<CartItemDetail> Item { get; set; } = new();
        public int TotalCartQuantity { get; set; }
        public decimal TotalCartPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CartResponse
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public string User { get; set; } = default!;
        public List<CartItemResponse> CartItems { get; set; } = new();
        public int TotalCartItem { get; set; }
        public int TotalCartQuantity { get; set; }
        public decimal TotalCartPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
