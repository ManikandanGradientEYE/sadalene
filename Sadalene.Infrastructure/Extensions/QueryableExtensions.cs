using Microsoft.EntityFrameworkCore;
using Sadalene.Core.Common;

namespace Sadalene.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
