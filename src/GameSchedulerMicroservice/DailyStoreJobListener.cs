using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSchedulerMicroservice;
using GameSchedulerMicroservice.Repositories;
using Microsoft.Extensions.Logging;
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
            var logger = (ILogger)dataMap["logger"];
            var message = gameRepo.GetNextGames();

            messageBusSetup.Publish(message);

            await Task.Delay(100);
            logger.LogDebug("Messaged published to RabbitMQ!");
            await Task.Delay(0);
        }
    }
}
