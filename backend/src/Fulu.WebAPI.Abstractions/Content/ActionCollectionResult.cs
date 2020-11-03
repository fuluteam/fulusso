using System.Collections;
using System.Collections.Generic;

namespace Fulu.WebAPI.Abstractions
{
    public class ActionCollectionResult<T> : ActionObjectResult<T>
    {
        public ActionCollectionResult()
        {
            Statistics = new Statistic();
        }
        /// <summary>
        /// 
        /// </summary>
        public Statistic Statistics { get; set; }
    }
}
