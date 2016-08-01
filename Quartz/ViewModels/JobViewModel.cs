using System;
using System.ComponentModel.DataAnnotations;

namespace Quartz.Presentation.ViewModels
{
    public class JobViewModel
    {
        public string Name { get; set; }

        public bool Selected { get; set; }

        [Display(Name = "Last Run")]
        public string LastRun { get; set; }

        [Display(Name = "Next Run")]
        public string NextRun { get; set; }

        public string Running { get; set; }

        [Display(Name = "Times Triggerered")]
        public int TimesTriggered { get; set; }
    }
}