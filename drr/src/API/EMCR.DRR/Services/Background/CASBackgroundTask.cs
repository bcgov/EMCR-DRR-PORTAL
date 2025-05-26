namespace EMCR.DRR.API.Services.Background
{
    public class CASBackgroundTask : IBackgroundTask
    {
        public string Schedule => "0 0 8 * * *"; //once a day at 8 UTC = 12am PST = 1am PDT
        public int InitialDelay => 30;

        public int DegreeOfParallelism => 1;

        public TimeSpan InactivityTimeout => TimeSpan.FromMinutes(5);

        private readonly ILogger _logger;

        public CASBackgroundTask(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CASBackgroundTask>();
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            _logger.LogInformation("Task run...");
        }
    }
}
