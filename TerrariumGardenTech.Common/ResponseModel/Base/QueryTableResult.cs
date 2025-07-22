using TerrariumGardenTech.Common.RequestModel.Base;

namespace TerrariumGardenTech.Common.ResponseModel.Base;

public class QueryTableResult
{
    public QueryTableResult()
    {
    }

    public QueryTableResult(GetQueryableQuery query, IEnumerable<object>? results = null, int? totalRecords = null)
    {
        Results = results ?? [];
        IsPagination = query.Pagination.IsPagingEnabled;
        IncludeProperties = query.IncludeProperties;

        if (!IsPagination) return;

        PageNumber = query.Pagination.PageNumber;
        PageSize = query.Pagination.PageSize;
        TotalRecords = totalRecords;
        if (totalRecords != null)
            TotalPages = (int)Math.Ceiling(totalRecords.Value / (double)query.Pagination.PageSize);
    }

    public IEnumerable<object>? Results { get; }
    public string[]? IncludeProperties { get; protected set; }


    public int? TotalPages { get; protected set; }
    public int? TotalRecords { get; protected set; }

    public int? PageNumber { get; protected set; }

    public int? PageSize { get; protected set; }

    public bool IsPagination { get; protected set; }
}