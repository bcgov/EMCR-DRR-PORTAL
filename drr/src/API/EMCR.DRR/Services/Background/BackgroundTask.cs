using System.Reflection;
using Cronos;
using Medallion.Threading;

namespace EMCR.DRR.API.Services.Background
{
    public interface IBackgroundTask
    {
        public string Schedule { get; }
        public int DegreeOfParallelism { get; }
        public int InitialDelay { get; }
        TimeSpan InactivityTimeout { get; }

        public Task ExecuteAsync(CancellationToken cancellationToken);
    }

    internal class BackgroundTask<T> : BackgroundService
        where T : IBackgroundTask
    {
        private readonly IServiceProvider serviceProvider;
        private readonly CronExpression schedule;
        private readonly TimeSpan startupDelay;
        private readonly bool enabled;
        private readonly IDistributedSemaphore semaphore;
        private long runNumber = 0;
        private readonly ILogger _logger;

        public BackgroundTask(IServiceProvider serviceProvider, IDistributedSemaphoreProvider distributedSemaphoreProvider, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
            this.serviceProvider = serviceProvider;
            using (var scope = serviceProvider.CreateScope())
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>().GetSection($"backgroundtask:{typeof(T).Name}");
                var task = scope.ServiceProvider.GetRequiredService<T>();
                var appName = Environment.GetEnvironmentVariable("APP_NAME") ?? Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;

#pragma warning disable CS8604 // Possible null reference argument.
                schedule = CronExpression.Parse(configuration.GetValue("schedule", task.Schedule), CronFormat.IncludeSeconds);
#pragma warning restore CS8604 // Possible null reference argument.
                var delaySeconds = configuration.GetValue("initialDelay", task.InitialDelay);
                startupDelay = TimeSpan.FromSeconds(delaySeconds);
                enabled = configuration.GetValue("enabled", false);
                var degreeOfParallelism = configuration.GetValue("degreeOfParallelism", task.DegreeOfParallelism);

                if (!string.IsNullOrEmpty(appName)) appName += "-";
                semaphore = distributedSemaphoreProvider.CreateSemaphore($"{appName}backgroundtask:{typeof(T).Name}", degreeOfParallelism);

                if (enabled)
                {
                    _logger.LogInformation("starting {0}: initial delay {1}, schedule: {2}, parallelism: {3}", typeof(T).Name, this.startupDelay, this.schedule.ToString(), task.DegreeOfParallelism);
                }
                else
                {
                    _logger.LogInformation($"background task is disabled, check configuration flag 'backgroundTask:{typeof(T).Name}'");
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!enabled) return;

            await Task.Delay(startupDelay, stoppingToken);

            var nextExecutionDelay = CalculateNextExecutionDelay(DateTime.UtcNow);

            IDistributedSynchronizationHandle? handle = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                runNumber++;
                using (var scope = serviceProvider.CreateScope())
                {
                    var task = scope.ServiceProvider.GetRequiredService<T>();

                    _logger.LogInformation("next {0} run in {1}s", typeof(T).Name, nextExecutionDelay.TotalSeconds);

                    try
                    {
                        // get a lock
                        handle = await semaphore.TryAcquireAsync(TimeSpan.FromSeconds(5), stoppingToken);

                        // wait in the lock
                        await Task.Delay(nextExecutionDelay, stoppingToken);
                        if (handle == null)
                        {
                            // no lock
                            _logger.LogInformation("skipping {0} run {1}", typeof(T).Name, runNumber);
                            continue;
                        }
                        try
                        {
                            // do work
                            _logger.LogInformation("executing {0} run # {1}", typeof(T).Name, runNumber);
                            await task.ExecuteAsync(stoppingToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("error in {0} run # {1}: {2}", typeof(T).Name, runNumber, e.Message);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("unhandled error in {0}: {1}", typeof(T).Name, e.Message);
                    }
                    finally
                    {
                        nextExecutionDelay = CalculateNextExecutionDelay(DateTime.UtcNow);
                        // release the lock
                        if (handle != null) await handle.DisposeAsync();
                    }
                }
            }
        }

        private TimeSpan CalculateNextExecutionDelay(DateTime utcNow)
        {
            var nextDate = schedule.GetNextOccurrence(utcNow);
            if (nextDate == null) throw new InvalidOperationException("Cannot calculate the next execution date, stopping the background task");

            return nextDate.Value.Subtract(utcNow);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
