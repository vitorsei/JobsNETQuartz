using System;
using System.Collections.Generic;
using System.Threading;

namespace Quartz.Application
{
    public class OrderIntregrationAppService
    {
        public IList<int> GetNewOrders()
        {
            Thread.Sleep(TimeSpan.FromSeconds(45));

            return new List<int> { 123, 346, 789};
        }
    }
}
