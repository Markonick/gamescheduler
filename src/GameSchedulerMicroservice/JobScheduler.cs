﻿using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace GameScheduler
{
    public class JobScheduler
    {
        public async Task Start()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            // and start it off
            await scheduler.Start();
            var storeDailyGamesJob = JobBuilder.Create<StoreDailyGamesJob>().Build();
            //var publishGamesJob = JobBuilder.Create<PublishGamesJob>().Build();

            /*var storeDailyGamesTrigger = TriggerBuilder.Create()
                    .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(22, 26)))
                    .Build();

            var publishGamesJobTrigger = TriggerBuilder.Create()
                    .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(14, 14)))
                    .Build();*/
            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger7", "group1")
                .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(5)
                .RepeatForever())
                .EndAt(DateBuilder.DateOf(22, 50, 0))
                .Build();

            await scheduler.ScheduleJob(storeDailyGamesJob, trigger);
            //await scheduler.ScheduleJob(publishGamesJob, publishGamesJobTrigger); // some sleep to show what's happening

            await Task.Delay(TimeSpan.FromSeconds(30));
        }

        public async Task Stop()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Shutdown();
        }
    }
}
