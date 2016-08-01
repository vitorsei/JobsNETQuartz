using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quartz.Presentation.ViewModels
{
    public class JobDetailViewModel
    {
        [Required]
        public string Name { get; set; }

        [Range(1, 86400)]
        public int Interval { get; set; }

        [DataType(DataType.Time, ErrorMessage = "Invalid dateTime format")]
        [Display(Name = "First run")]
        public TimeSpan FirstRun { get; set; }

        [DataType(DataType.Time, ErrorMessage = "Invalid dateTime format")]
        [Display(Name = "Last run")]
        public TimeSpan LastRun { get; set; }

        public IDictionary<string, string> Parameters { get; set; }
    }
}