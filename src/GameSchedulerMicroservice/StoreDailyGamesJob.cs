using System;
using System.Threading.Tasks;
using GameSchedulerMicroservice.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;

namespace GameSchedulerMicroservice
{
    public class StoreDailyGamesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var gameRepo = (IGameScheduleRepository) dataMap["gameRepo"];
            var logger = (ILogger)dataMap["logger"];

            gameRepo.StoreDailySchedule();

            logger.LogDebug("Storing of Daily Game Schedule to MongoDb complete!");
            await Task.Delay(0);
        }
    }
}
