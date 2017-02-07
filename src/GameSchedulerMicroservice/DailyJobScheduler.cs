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

            //Store every day at 00:00
            var storeDailyGamesJob = JobBuilder.Create<StoreDailyGamesJob>()
                .WithIdentity("job1")
                .Build();
            
            //Check continuously that there is an upcoming game in 10 mins
            var publishGamesJob = JobBuilder.Create<PublishGamesJob>()
                .WithIdentity("job2")
                .Build();

            storeDailyGamesJob.JobDataMap["gameRepo"] = _gameRepository;
            publishGamesJob.JobDataMap["gameRepo"] = _gameRepository;
            publishGamesJob.JobDataMap["messageBusSetup"] = _messageBusSetup;

            var triggerTime= TimeOfDay.HourMinuteAndSecondOfDay(00, 50, 20);

            var storeDailyGamesTrigger = TriggerBuilder.Create()
                    .WithDailyTimeIntervalSchedule(s => s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(triggerTime))
                    .Build();

            var myJobListener = new DailyStoreJobListener();
            myJobListener.Name = "MyJobListener1";
            scheduler.ListenerManager.AddJobListener(myJobListener, KeyMatcher<JobKey>.KeyEquals(new JobKey("PublishGamesJob", "PublishGamesJobGroup")));
            
            await scheduler.ScheduleJob(storeDailyGamesJob, storeDailyGamesTrigger);
        }

        public async Task Stop()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Shutdown();
        }
    }
}
