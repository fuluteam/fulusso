using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ICH.TransferJob
{
    public interface IBackgroundRunService
    {
        Task Execute(CancellationToken cancellationToken);
        void Transfer<T>(Expression<Func<T, Task>> expression);
        void Transfer(Expression<Action> expression);
    }
}