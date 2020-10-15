using System;
using System.Collections.Generic;
using System.Linq;

namespace Fulu.Core.Common
{
    /// <summary>
    /// 分页列表
    /// </summary>
    /// <typeparam name="T">The type of the data to page</typeparam>
    public class PagedList<T>: IPagedList<T>
    {
        public PagedList()
        {

        }
        public PagedList(IList<T> items, int pageIndex, int pageSize, int totalCount)
        {
            Current = pageIndex;
            PageSize = pageSize;
            Total = totalCount;
            PageTotal = (int)Math.Ceiling(totalCount / (double)pageSize);
            List = items;
        }

        //internal PagedList() { }

        public int Current { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }

        public int PageTotal { get; set; }

        public IEnumerable<T> List { get; set; }

        public static PagedList<T> Create(IPagedList<T> source)
        {
            if (source is PagedList<T> same)
                return same;
            return new PagedList<T>(source.List.ToList(), source.Current, source.PageSize, source.Total);
        }
    }
}
