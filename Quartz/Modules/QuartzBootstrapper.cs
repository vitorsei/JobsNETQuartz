using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Jobs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web.Hosting;

namespace Quartz.Presentation.Modules
{
    public sealed class QuartzBootstrapper : IRegisteredObject
    {
        private static QuartzBootstrapper _instance;
        private static object _lockObject = new object();
        private bool _started;
        private JobConfiguration _jobConfig;
        private IScheduler _scheduler = null;
        private IDictionary<string, Action> _jobs;

        private QuartzBootstrapper()
        {
        }
        
        #region Public Methods
        public static QuartzBootstrapper Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                        _instance = new QuartzBootstrapper();
                }

                return _instance;
            }
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_started)
                    return;

                _started = true;

                SetInitialConfig();

                CreateJobs();

                Log.Information("QuartzBootsrapper has started.");
            }
        }

        public void Stop(bool immediate)
        {
            lock (_lockObject)
            {
                if (_scheduler != null)
                    _scheduler.Shutdown();

                HostingEnvironment.UnregisterObject(this);
            }
        }

        public IScheduler Scheduler
        {
            get { return _scheduler; }
        }
        #endregion

        #region Internal Methods

        internal void ScheduleJob(IJobDetail job, ITrigger trigger)
        {
            _scheduler.ScheduleJob(job, trigger);
        }

        internal void ResetJob(string jobName)
        {
            _scheduler.ResumeTrigger(new TriggerKey(jobName));
            LogUserChanges(jobName, "reset");
        }

        internal void PauseJob(string name)
        {
            _scheduler.PauseTrigger(new TriggerKey(name));
            LogUserChanges(name, "paused");
        }

        internal IList<IJobExecutionContext> GetRunningJobs()
        {
            return _scheduler.GetCurrentlyExecutingJobs();
        }

        internal void Reschedule(string jobName, int interval, TimeSpan firstExecution, 
            TimeSpan lastExecution, IDictionary<string, string> parameters)
        {
            ITrigger oldTrigger = _scheduler.GetTrigger(new TriggerKey(jobName));
            var newTrigger = new
            {
                FirstExecution = firstExecution,
                LastExecution = lastExecution,
                Interval = interval
            };

            JobDataMap oldDataMap = _scheduler.GetJobDetail(new JobKey(jobName)).JobDataMap;
            JobDataMap newDataMap = new JobDataMap();
            if (parameters != null)
                newDataMap = new JobDataMap((IDictionary<string, object>)parameters.ToDictionary(pair => pair.Key, pair => (object)pair.Value));

            _scheduler.DeleteJob(new JobKey(jobName));
            _jobs[jobName].DynamicInvoke();

            var machineIP = UserInfo.GetMachineIP();
            var usuario = UserInfo.GetUserInfo();
            var job = jobName.Replace("Quartz", "");

            var log = Serilog.Log.Logger
                       .ForContext("Job", job)
                       .ForContext("User", usuario == null ? "" : usuario.UserName)
                       .ForContext("Email", usuario == null ? "" : usuario.Email)
                       .ForContext("IP", machineIP)
                       .ForContext("OldParameters", oldDataMap)
                       .ForContext("NewParameters", newDataMap)
                       .ForContext("OldTrigger", oldTrigger, true)
                       .ForContext("NewTrigger", newTrigger, true);

            log.Information("Job config modified");
        }

        #endregion

        #region Private Methods

        private void CreateJobs()
        {
            _jobs = new Dictionary<string, Action>();

            _jobs[typeof(OrderIntegrationJob).Name] = new Action(CreateJob<OrderIntegrationJob>);
            _jobs[typeof(ShippingIntegrationJob).Name] = new Action(CreateJob<ShippingIntegrationJob>);

            _jobs[typeof(OrderIntegrationJob).Name].DynamicInvoke();
            _jobs[typeof(ShippingIntegrationJob).Name].DynamicInvoke();

            PauseAllJobsIfDebug();
        }

        private void CreateJob<T>() where T : IJob
        {
            string jobName = typeof(T).Name;
            var jobConfiguration = _jobConfig.Get(jobName);
            Dictionary<string, object> firstRun = (Dictionary<string, object>)jobConfiguration["FirstRun"];
            Dictionary<string, object> lastRun = (Dictionary<string, object>)jobConfiguration["LastRun"];
            JobDataMap jobDataMap = new JobDataMap((IDictionary<string, object>)jobConfiguration["Parameters"]);

            IJobDetail job = JobBuilder.Create<T>()
                .UsingJobData(jobDataMap)
                .WithIdentity(jobName)
                .StoreDurably()
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                              .WithIdentity(jobName)
                              .WithDailyTimeIntervalSchedule
                                  (s =>
                                      s.WithIntervalInSeconds((int)jobConfiguration["Interval"])
                                      .OnEveryDay()
                                      .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay((int)firstRun["Hours"], (int)firstRun["Minutes"]))
                                      .EndingDailyAt(TimeOfDay.HourAndMinuteOfDay((int)lastRun["Hours"], (int)lastRun["Minutes"]))
                                  )
                              .Build();

            ScheduleJob(job, trigger);
        }

        private void SetInitialConfig()
        {
            HostingEnvironment.RegisterObject(this);
            var properties = new NameValueCollection { { "quartz.threadPool.threadCount", "10" } };
            var schedulerFactory = new StdSchedulerFactory(properties);
            _scheduler = schedulerFactory.GetScheduler();
            _scheduler.Start();

            _jobConfig = new JobConfiguration();
        }

        private void LogUserChanges(string jobName, string message)
        {
            var machineIP = UserInfo.GetMachineIP();
            var user = UserInfo.GetUserInfo();
            var job = jobName.Replace("Quartz", "");

            var log = Serilog.Log.Logger
                       .ForContext("Job", (object)job)
                       .ForContext("User", user == null ? "" : user.UserName)
                       .ForContext("Email", user == null ? "" : user.Email)
                       .ForContext("IP", machineIP);

            log.Information(string.Format("Job {0} {1}", (object)job, message));
        }

        [Conditional("DEBUG")]
        private void PauseAllJobsIfDebug()
        {
            var jobKeys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var key in jobKeys)
            {
                _scheduler.PauseTrigger(new TriggerKey(key.Name));
            }
        }

        #endregion
    }
}