using static TerrariumGardenTech.Common.Enums.CommonData;

namespace TerrariumGardenTech.Common.ResponseModel.TerrariumLayout;


public class TerrariumLayoutResponseDto
{
    public int LayoutId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string LayoutName { get; set; }
    public LayoutStatus Status { get; set; }
    public decimal? FinalPrice { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    // Terrarium details
    public TerrariumDetailsDto Terrarium { get; set; }
}

public class TerrariumDetailsDto
{
    public int TerrariumId { get; set; }
    public string TerrariumName { get; set; }
    public string Description { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; }

    // Environment, Shape, TankMethod
    public string EnvironmentName { get; set; }
    public string ShapeName { get; set; }
    public string TankMethodDescription { get; set; }

    // Images and Accessories
    public List<string> ImageUrls { get; set; } = new();
    public List<string> AccessoryNames { get; set; } = new();
}

public class TerrariumFullDetailsDto
{
    public int TerrariumId { get; set; }
    public string TerrariumName { get; set; }
    public string Description { get; set; }
    public string? BodyHTML { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Related Entities
    public EnvironmentDto? Environment { get; set; }
    public ShapeDto? Shape { get; set; }
    public TankMethodDto? TankMethod { get; set; }

    // Collections
    public List<TerrariumImageDto> Images { get; set; } = new();
    public List<TerrariumAccessoryDto> Accessories { get; set; } = new();

    // Computed Properties
    public string PriceRange => $"{MinPrice:N0} - {MaxPrice:N0} VND";
    public int ImageCount => Images.Count;
    public int AccessoryCount => Accessories.Count;
    public string PrimaryImageUrl => Images.FirstOrDefault()?.ImageUrl ?? "";
}

public class EnvironmentDto
{
    public int EnvironmentId { get; set; }
    public string EnvironmentName { get; set; }
    public string? Description { get; set; }
}

public class ShapeDto
{
    public int ShapeId { get; set; }
    public string ShapeName { get; set; }
    public string? Description { get; set; }
}

public class TankMethodDto
{
    public int TankMethodId { get; set; }
    public string TankMethodDescription { get; set; }
}

public class TerrariumImageDto
{
    public int TerrariumImageId { get; set; }
    public string ImageUrl { get; set; }
}

public class TerrariumAccessoryDto
{
    public int AccessoryId { get; set; }
    public string AccessoryName { get; set; }
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
}
public class TerrariumLayoutDto
{
    public int LayoutId { get; set; }
    public string LayoutName { get; set; }
    public string Status { get; set; }
    public decimal? FinalPrice { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    // Chỉ IDs - FE sẽ gọi API riêng để lấy details
    public int UserId { get; set; }
    public int TerrariumId { get; set; }
    public int? ReviewedBy { get; set; }

    // Minimal info cần thiết cho display
    public DateTime? ReviewDate { get; set; }
    public string? ReviewNotes { get; set; }
}

public class TerrariumLayoutDetailDto
{
    public int LayoutId { get; set; }
    public string LayoutName { get; set; }
    public string Status { get; set; }
    public decimal? FinalPrice { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    // IDs only
    public int UserId { get; set; }
    public int TerrariumId { get; set; }
    public int? ReviewedBy { get; set; }

    // Review info
    public DateTime? ReviewDate { get; set; }
    public string? ReviewNotes { get; set; }
}
