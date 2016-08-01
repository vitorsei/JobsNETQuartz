using Quartz.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class OrderIntegrationJob : IJob
    {
        private readonly OrderIntregrationAppService _orderIntregrationAppService;
        private readonly string _jobName;
        private const int MAX_RETRIES = 2;

        public OrderIntegrationJob()
        {
            _orderIntregrationAppService = new OrderIntregrationAppService();
            _jobName = this.GetType().Name.Replace("Job", "");
        }

        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int delay = int.Parse(dataMap.GetString("Delay"));

            var watch = Stopwatch.StartNew();

            Func<IList<int>> getNewOrders = _orderIntregrationAppService.GetNewOrders;
            getNewOrders = getNewOrders.RetryIfFailed(MAX_RETRIES);

            IList<int> orders = getNewOrders();

            watch.Stop();

            if (orders.Count > 0)
            {
                var log = Serilog.Log.Logger
                        .ForContext("Job", _jobName)
                        .ForContext("Orders", orders, true)
                        .ForContext("TimeElpased", watch.Elapsed);
                log.Information("{Count} new orders were integrated.", orders.Count);
            }
        }
    }
}
