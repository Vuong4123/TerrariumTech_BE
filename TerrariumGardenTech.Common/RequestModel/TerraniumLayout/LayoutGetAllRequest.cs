using static TerrariumGardenTech.Common.Enums.CommonData;

namespace TerrariumGardenTech.Common.RequestModel.TerraniumLayout;

public class LayoutGetAllRequest : PaginationRequest
{
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class UpdateLayoutRequest
{
    public string? LayoutName { get; set; }
    public int? TerrariumId { get; set; }
    public string? Status { get; set; }
    public decimal? FinalPrice { get; set; }
    public int? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
}


public class GetLayoutRequest
{
    public int UserId { get; set; } // UserId of the user making the request
    public int LayoutId { get; set; }
    public string? UserRole { get; set; }
}
public class ReviewLayoutRequest
{
    public string Status { get; set; }
    public decimal? FinalPrice { get; set; }
    public string? ReviewNotes { get; set; }
}

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
public class CreateLayoutRequest
{
    public int userId { get; set; }
    public string LayoutName { get; set; }
    public int TerrariumId { get; set; } // Client chọn terrarium có sẵn
}
