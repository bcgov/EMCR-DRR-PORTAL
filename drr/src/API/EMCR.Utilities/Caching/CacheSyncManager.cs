﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EMCR.Utilities.Caching
{
    internal class CacheSyncManager : ConcurrentDictionary<string, SemaphoreSlim>
    {
        ILogger<CacheSyncManager> logger;

        public CacheSyncManager(ILogger<CacheSyncManager> logger)
        {
            this.logger = logger;
        }

        public string Schedule => "* */1 * * * *";

        public int DegreeOfParallelism => -1;

        public TimeSpan InitialDelay => TimeSpan.FromSeconds(10);

        public TimeSpan InactivityTimeout => TimeSpan.FromMinutes(5);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            logger.LogDebug("start cache lockers prunning");
            var prunned = 0;
            foreach (var item in this)
            {
                if (item.Value.CurrentCount == 1 && TryRemove(item.Key, out var locker))
                {
                    locker.Dispose();
                    prunned++;
                }
            }
            logger.LogDebug("end cache lockers prunning: {0} prunned", prunned);
        }
    }
}