using System.Collections.Generic;

namespace Quartz.Presentation.ViewModels
{
    public class JobListViewModel
    {
        public JobListViewModel()
        {
            Jobs = new List<JobViewModel>();
        }

        public IList<JobViewModel> Jobs { get; set; }
    }
}