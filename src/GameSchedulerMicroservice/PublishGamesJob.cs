using System;
using System.Threading.Tasks;
using GameSchedulerMicroservice.Repositories;
using Quartz;

namespace GameSchedulerMicroservice
{
    public class PublishGamesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var gameRepo = (IGameScheduleRepository)dataMap["gameRepo"];
            var messageBusSetup = (IMessageBusSetup)dataMap["messageBusSetup"];

            var message = gameRepo.GetNextGames();
            messageBusSetup.Publish(message);

            await Task.Delay(1);
        }
    }
}
