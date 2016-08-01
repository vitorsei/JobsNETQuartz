using Quartz.Impl.Matchers;
using Quartz.Presentation.Modules;
using Quartz.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Quartz.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly JobConfiguration _jobConfiguration;

        public HomeController()
        {
            _jobConfiguration = new JobConfiguration();
        }

        public ActionResult Index()
        {
            JobListViewModel allJobs = new JobListViewModel();

            var jobsRunning = QuartzBootstrapper.Instance.Scheduler.GetCurrentlyExecutingJobs();
            var allJobKeys = QuartzBootstrapper.Instance.Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

            foreach (var key in allJobKeys)
            {
                var jobTriggers = QuartzBootstrapper.Instance.Scheduler.GetTriggersOfJob(key);

                allJobs.Jobs.Add(new JobViewModel
                {
                    Selected = false,
                    Name = key.Name,
                    LastRun = GetLastJobRun(jobTriggers),
                    NextRun = GetNextJobRun(jobTriggers),
                    TimesTriggered = (jobTriggers.FirstOrDefault() as IDailyTimeIntervalTrigger).TimesTriggered,
                    Running = jobsRunning.Any(x => x.JobDetail.Key == key).BooleanDisplayValuesAsYesNo()
                });
            }

            return View(allJobs);
        }

        [HttpPost]
        public ActionResult Index(string submitButton, JobListViewModel jobsList)
        {            
            if (submitButton == "Stop")
            {
                foreach (var job in jobsList.Jobs.Where(x => x.Selected))
                {
                    QuartzBootstrapper.Instance.PauseJob(job.Name);
                }
            }
            else
            {
                foreach (var job in jobsList.Jobs.Where(x => x.Selected))
                {
                    QuartzBootstrapper.Instance.ResetJob(job.Name);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Detail(string jobName)
        {
            JobDetailViewModel jobViewModel = GetJobDetail(jobName);

            return View(jobViewModel);
        }

        public ActionResult Edit(string name)
        {
            JobDetailViewModel jobViewModel = GetJobDetail(name);

            return View(jobViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JobDetailViewModel job)
        {
            if (ModelState.IsValid)
            {
                _jobConfiguration.Edit(job.Name, job.Interval, job.FirstRun, job.LastRun, job.Parameters);
                QuartzBootstrapper.Instance.Reschedule(job.Name, job.Interval, job.FirstRun, job.LastRun, job.Parameters);

                return RedirectToAction("Index");
            }

            return View(job);
        }

        #region Private Methods
        private static JobDetailViewModel GetJobDetail(string jobName)
        {
            IDictionary<string, string> jobConfiguration = new Dictionary<string, string>();

            var jobDetail = QuartzBootstrapper.Instance.Scheduler.GetJobDetail(new JobKey(jobName));
            foreach (var item in jobDetail.JobDataMap.GetKeys())
            {
                jobConfiguration.Add(item, jobDetail.JobDataMap.GetString(item));
            }

            IDailyTimeIntervalTrigger triggerDetails =
                QuartzBootstrapper.Instance.Scheduler.GetTrigger(new TriggerKey(jobName)) as
                    IDailyTimeIntervalTrigger;

            JobDetailViewModel jobViewModel = new JobDetailViewModel
            {
                Name = jobName,
                Interval = triggerDetails.RepeatInterval,
                FirstRun = new TimeSpan(triggerDetails.StartTimeOfDay.Hour
                                                , triggerDetails.StartTimeOfDay.Minute
                                                , triggerDetails.StartTimeOfDay.Second),
                LastRun = new TimeSpan(triggerDetails.EndTimeOfDay.Hour
                                                , triggerDetails.EndTimeOfDay.Minute
                                                , triggerDetails.EndTimeOfDay.Second),
                Parameters = jobConfiguration

            };
            return jobViewModel;
        }

        private string GetLastJobRun(IList<ITrigger> jobTriggers)
        {
            var lastRun = jobTriggers.DefaultIfEmpty().Max(q => q.GetPreviousFireTimeUtc());
            if (lastRun.HasValue)
                return lastRun.Value.LocalDateTime.ToString();

            return string.Empty;
        }

        private string GetNextJobRun(IList<ITrigger> triggersDoJob)
        {
            if (QuartzBootstrapper.Instance.Scheduler.GetTriggerState(triggersDoJob.FirstOrDefault().Key) ==
                TriggerState.Paused)
                return string.Empty;

            var nextRun = triggersDoJob.DefaultIfEmpty().Min(q => q.GetNextFireTimeUtc());
            if (nextRun.HasValue)
                return nextRun.Value.LocalDateTime.ToString();

            return string.Empty;
        }

        #endregion
    }
}