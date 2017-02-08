using System;
using System.Threading.Tasks;
using GameScheduler;
using GameSchedulerMicroservice.Repositories;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace GameSchedulerMicroservice
{
    public class DailyJobScheduler
    {
        private readonly IGameScheduleRepository _gameRepository;
        private readonly IMessageBusSetup _messageBusSetup;

        public DailyJobScheduler(IGameScheduleRepository gameRepository, IMessageBusSetup messageBusSetup)
        {
            _gameRepository = gameRepository;
            _messageBusSetup = messageBusSetup;
        }

        public async Task Start()
        {
            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            await scheduler.Start();

            //Jobs
            var storeDailyGamesJob = JobBuilder.Create<StoreDailyGamesJob>()
                .WithIdentity("job1")
                .Build();
            
            var publishGamesJob = JobBuilder.Create<PublishGamesJob>()
                .WithIdentity("job2")
                .Build();

            //Triggers
            var storeDailyGamesTrigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24)
                .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(19, 24))
                .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")))
                .Build();

            //Pass params to jobs through jobdatamap
            storeDailyGamesJob.JobDataMap["gameRepo"] = _gameRepository;
            publishGamesJob.JobDataMap["gameRepo"] = _gameRepository;
            publishGamesJob.JobDataMap["messageBusSetup"] = _messageBusSetup;

            var myJobListener = new DailyStoreJobListener {Name = "MyJobListener1"};
            scheduler.ListenerManager.AddJobListener(myJobListener, KeyMatcher<JobKey>.KeyEquals(new JobKey("job1")));
            
            await scheduler.ScheduleJob(storeDailyGamesJob, storeDailyGamesTrigger);
        }

        public async Task Stop()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Shutdown();
        }
    }
}
