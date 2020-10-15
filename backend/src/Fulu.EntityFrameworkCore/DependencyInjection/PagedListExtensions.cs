namespace Fulu.EntityFrameworkCore.DependencyInjection
{

    public static class PagedListExtensions
    {
        ///// <summary>
        ///// 分页查询
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="query"></param>
        ///// <param name="pageIndex">1为起始页</param>
        ///// <param name="pageSize"></param>
        ///// <param name="cancellationToken"></param>
        ///// <returns></returns>
        //public static async Task<PagedList<T>> ToPagedListAsync<T>(
        //    this IQueryable<T> query,
        //    int pageIndex,
        //    int pageSize,
        //    CancellationToken cancellationToken = default)
        //{
        //    if (pageIndex < 1)
        //    {
        //        throw new ArgumentOutOfRangeException(nameof(pageIndex));
        //    }

        //    int realIndex = pageIndex - 1;

        //    int count = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        //    List<T> items = await query.Skip(realIndex * pageSize)
        //        .Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);

        //    return new PagedList<T>(items, pageIndex, pageSize, count);
        //}
    }
}
