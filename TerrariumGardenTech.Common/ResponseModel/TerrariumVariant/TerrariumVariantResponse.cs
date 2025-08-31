using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.ResponseModel.TerrariumVariant
{
    // TerrariumVariantResponse.cs
    public class TerrariumVariantResponse
    {
        public int TerrariumVariantId { get; set; }
        public int TerrariumId { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? UrlImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ✅ THÊM: Danh sách accessories
        public List<VariantAccessoryResponse> Accessories { get; set; } = new List<VariantAccessoryResponse>();
    }

    // VariantAccessoryResponse.cs
    public class VariantAccessoryResponse
    {
        public int AccessoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } // Số lượng cần cho variant này
        public string? ImageUrl { get; set; }
    }
}
