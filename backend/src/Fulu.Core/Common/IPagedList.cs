using System.Collections.Generic;

namespace Fulu.Core.Common
{
    public interface IPagedList<out T>
    {
        int Current { get; }

        int PageSize { get; }

        int Total { get; }

        int PageTotal { get; }

        IEnumerable<T> List { get; }
    }
}
