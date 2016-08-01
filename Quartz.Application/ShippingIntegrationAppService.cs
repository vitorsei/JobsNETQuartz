using System;
using System.Collections.Generic;
using System.Threading;

namespace Quartz.Application
{
    public class ShippingIntegrationAppService
    {
        public IList<int> UpdateTrackings()
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));

            return new List<int> { 123, 456, 789 };
        }
    }
}
