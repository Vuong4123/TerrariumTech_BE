namespace TerrariumGardenTech.Common.RequestModel.Pagination;

public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Validation
    public int ValidPage => Page <= 0 ? 1 : Page;
    public int ValidPageSize => PageSize <= 0 ? 10 : (PageSize > 100 ? 100 : PageSize);
}