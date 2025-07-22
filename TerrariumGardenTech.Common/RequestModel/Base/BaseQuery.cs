using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Common.RequestModel.Base;

public abstract class BaseQuery
{
}

public class PaginationParameters
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    [Range(1, int.MaxValue)]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    [Range(1, 100)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
    }

    public bool IsPagingEnabled { get; set; } = true;
}

public class GetQueryableQuery : BaseQuery
{
    private PaginationParameters _pagination = new();

    public PaginationParameters Pagination
    {
        get => _pagination;
        set => _pagination = value ?? new PaginationParameters();
    }

    public string[]? IncludeProperties { get; set; }
}

public class GetByIdQuery : BaseQuery
{
    [Required] public new Guid Id { get; set; }

    public string[]? IncludeProperties { get; set; }
}

public class GetAllQuery : GetQueryableQuery
{
}