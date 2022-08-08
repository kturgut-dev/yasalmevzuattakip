using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KYS.YasalMevzuatTakip
{
    public class KYSJobScheduler
    {
        private static IScheduler ISchedulerStart()
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = (IScheduler)schedFact.GetScheduler().Result;
            if (!sched.IsStarted)
                sched.Start();

            return sched;
        }

        public static void Start()
        {
            IScheduler sched = ISchedulerStart();

            IJobDetail Gorev = JobBuilder.Create<KYSJob>().WithIdentity("KYSJobNew", null).Build();

            ITrigger TriggerGorev = TriggerBuilder.Create().WithIdentity("KYSJobNew", null)
                           .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(11, 00))
                           .StartAt(DateTime.UtcNow)
                           .Build();

            // Tetiklenme süresi buradan ayarlanıyor //hersabah 6 da tetiklenecek
            sched.ScheduleJob(Gorev,TriggerGorev);
        }
    }
}
