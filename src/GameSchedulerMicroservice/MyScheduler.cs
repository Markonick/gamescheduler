using System;
using System.Threading.Tasks;
using GameSchedulerMicroservice.Helpers;
using GameSchedulerMicroservice.Repositories;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace GameSchedulerMicroservice
{
    public class MyScheduler
    {
        private readonly IGameScheduleRepository _gameRepository;

        public MyScheduler(IGameScheduleRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task Start()
        {
            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            await scheduler.Start();

            var storeDailyGamesJob = JobBuilder.Create<StoreDailyGamesJob>()
                .WithIdentity("job1")
                .Build();

            storeDailyGamesJob.JobDataMap["gameRepo"] = _gameRepository;
            //var publishGamesJob = JobBuilder.Create<PublishGamesJob>().Build();
            var storeDailyGamesTrigger = TriggerBuilder.Create()
                    .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(22, 11)))
                    .Build();

            var publishGamesJobTrigger = TriggerBuilder.Create()
                    .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(14, 14)))
                    .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(1)
                .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(storeDailyGamesJob, storeDailyGamesTrigger);
            //await scheduler.ScheduleJob(publishGamesJob, publishGamesJobTrigger); // some sleep to show what's happening

            //await Task.Delay(TimeSpan.FromSeconds(1));
        }

        public async Task Stop()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Shutdown();
        }
    }
}
