using System.Collections.Generic;
using System.Threading.Tasks;
using GameSchedulerMicroservice;

namespace GameScheduler.Repositories
{
    public interface IGameScheduleRepository
    {
        void StoreFullSchedule(dynamic response);
        void StoreDailySchedule();
        Message GetNextGame(string inOneHour);
    }
}