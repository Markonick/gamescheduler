using System;
using System.Threading.Tasks;
using GameSchedulerMicroservice.Repositories;
using Quartz;

namespace GameSchedulerMicroservice
{
    public class StoreDailyGamesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var gameRepo = (IGameScheduleRepository) dataMap["gameRepo"];
            gameRepo.StoreDailySchedule();
            Console.WriteLine("Hi from StoreDailyGameJOb");
            await Task.Delay(0);
        }
    }
}
