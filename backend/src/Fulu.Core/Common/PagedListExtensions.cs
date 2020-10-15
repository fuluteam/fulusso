using System;
using System.Collections.Generic;
using System.Linq;

namespace Fulu.Core.Common
{
    
    public static class PagedListExtensions
    {
        public static PagedList<T> ToPagedList<T>(IEnumerable<T> source, int pageIndex, int pageSize, int indexFrom)
        {
            if (pageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            pageIndex = pageIndex - 1;

            var items = source.ToList();

            return new PagedList<T>(items, pageIndex, pageSize, items.Count);
        }

        public static PagedList<T> ToPagedList<T>(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            if (pageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            pageIndex = pageIndex - 1;

            var items = source.ToList();

            return new PagedList<T>(items, pageIndex, pageSize, items.Count);
        }
    }
}
