using System.Collections.Generic;

namespace Fulu.WebAPI.Abstractions
{
    public class PageModel<T>
    {
        public int Current { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }

        public int PageTotal { get; set; }

        public List<T> List { get; set; }
    }
}
