using Quartz.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class ShippingIntegrationJob : IJob
    {
        private readonly ShippingIntegrationAppService _shippingIntegrationService;
        private readonly string _jobName;
        private const int MAX_RETRIES = 2;

        public ShippingIntegrationJob()
        {
            _shippingIntegrationService = new ShippingIntegrationAppService();
            _jobName = this.GetType().Name.Replace("Job", "");
        }

        public void Execute(IJobExecutionContext context)
        {
            var watch = Stopwatch.StartNew();

            Func<IList<int>> updateTrackings = _shippingIntegrationService.UpdateTrackings;
            updateTrackings = updateTrackings.RetryIfFailed(MAX_RETRIES);

            IList<int> orders = updateTrackings();

            watch.Stop();

            if (orders.Count > 0)
            {
                var log = Serilog.Log.Logger
                        .ForContext("Job", _jobName)
                        .ForContext("Orders", orders, true)
                        .ForContext("TimeElpased", watch.Elapsed);
                log.Information("{Count} trackings were updated.", orders.Count);
            }
        }
    }
}
