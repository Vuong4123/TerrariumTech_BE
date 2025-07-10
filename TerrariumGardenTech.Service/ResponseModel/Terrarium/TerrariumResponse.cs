using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.ResponseModel.Terrarium
{
    public class TerrariumAccessoryResponse
    {
        public int AccessoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        // Thêm các thuộc tính khác nếu có
    }
    public class TerrariumResponse
    {
        public int TerrariumId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public TerrariumStatusEnum Status { get; set; }
        public List<string> Environments { get; set; } = [];
        public List<string> Shapes { get; set; } = [];
        public List<string> TankMethods { get; set; } = [];
        public List<TerrariumAccessoryResponse> Accessories { get; set; } = []; // Thông tin chi tiết phụ kiện
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string BodyHTML { get; set; } = string.Empty;
        // Thêm thuộc tính mới để chứa danh sách ảnh
        public List<TerrariumImageResponse> TerrariumImages { get; set; } = [];
    }
    // public record EnvironmentDTO(
    //     int EnvironmentId,
    //     string EnvironmentName
    // );
    // public record ShapeDTO(
    //     int ShapeId,
    //     string ShapeName
    // );
    // public record TankMethodDTO(
    //     int TankMethodId,
    //     string TankMethodType
    // );
}
