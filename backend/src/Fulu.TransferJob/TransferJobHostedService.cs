using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ICH.TransferJob
{
    public class TransferJobHostedService:BackgroundService
    {
        private IBackgroundRunService _runService;
        public TransferJobHostedService(IBackgroundRunService runService)
        {
            _runService = runService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _runService.Execute(stoppingToken);
            }
        }
    }
}