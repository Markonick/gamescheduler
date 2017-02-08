using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSchedulerMicroservice;
using GameSchedulerMicroservice.Repositories;
using Quartz;
using Quartz.Listener;

namespace GameScheduler
{
    public class DailyStoreJobListener : IJobListener
    {
        public string Name { get; set; }

        public async Task JobExecutionVetoed(IJobExecutionContext context)
        {
            await Task.Delay(0);
        }

        public async Task JobToBeExecuted(IJobExecutionContext context)
        {
            await Task.Delay(0);
        }

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var gameRepo = (IGameScheduleRepository)dataMap["gameRepo"];
            var messageBusSetup = (IMessageBusSetup)dataMap["messageBusSetup"];

            var message = gameRepo.GetNextGames();
            messageBusSetup.Publish(message);

            await Task.Delay(1000);// JobWasExecuted(context, jobException);
            Console.WriteLine("Job Executed: {0}", context.JobDetail.Key);
            await Task.Delay(0);// JobWasExecuted(context, jobException);
        }
    }
}
