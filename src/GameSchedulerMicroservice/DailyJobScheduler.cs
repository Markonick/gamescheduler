using System;
using System.Threading.Tasks;
using GameScheduler;
using GameSchedulerMicroservice.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace GameSchedulerMicroservice
{
    public class DailyJobScheduler
    {
        private readonly IGameScheduleRepository _gameRepository;
        private readonly IMessageBusSetup _messageBusSetup;
        private readonly ILogger _logger;

        public DailyJobScheduler(IGameScheduleRepository gameRepository, IMessageBusSetup messageBusSetup, ILogger logger)
        {
            _gameRepository = gameRepository;
            _messageBusSetup = messageBusSetup;
            _logger = logger;
        }

        public async Task Start()
        {
            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            await scheduler.Start();

            //Jobs
            var storeDailyGamesJob = JobBuilder.Create<StoreDailyGamesJob>()
                .WithIdentity("storeDailyGamesJob")
                .Build();

            //Triggers
            var storeDailyGamesTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24)
                .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(15, 46))
                .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")))
                .Build();

            //Pass params to jobs through jobdatamap
            storeDailyGamesJob.JobDataMap["gameRepo"] = _gameRepository;
            storeDailyGamesJob.JobDataMap["messageBusSetup"] = _messageBusSetup;
            storeDailyGamesJob.JobDataMap["logger"] = _logger;

            var dailyStoreJobListener = new DailyStoreJobListener {Name = "dailyStoreJobListener" };
            scheduler.ListenerManager.AddJobListener(dailyStoreJobListener, KeyMatcher<JobKey>.KeyEquals(new JobKey("storeDailyGamesJob")));
            
            await scheduler.ScheduleJob(storeDailyGamesJob, storeDailyGamesTrigger);
        }

        public async Task Stop()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Shutdown();
        }
    }
}
