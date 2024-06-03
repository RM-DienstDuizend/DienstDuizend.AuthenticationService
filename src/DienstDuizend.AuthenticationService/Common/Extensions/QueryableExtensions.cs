using System.Linq.Expressions;
using DienstDuizend.AuthenticationService.Common.Dto;

namespace DienstDuizend.AuthenticationService.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition) return source.Where(predicate);
        return source;
    }
    
    public static PaginationResult<T> PaginateWithWrapper<T>(this IQueryable<T> source, int pageIndex, int pageSize, int totalCount)
    {
        if (pageIndex < 1) pageIndex = 1;

        if (pageSize < 1 || pageSize > 100) pageSize = 100;

        var paginatedData = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);

        return new PaginationResult<T>
        {
            Data = paginatedData.ToList(),
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalRecords = totalCount
        };
    }
    
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, int pageIndex, int pageSize)
    {
        if (pageIndex < 1) pageIndex = 1;

        if (pageSize < 1 || pageSize > 100) pageSize = 100;

        var paginatedData = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);

        return paginatedData;
    }
}