using System.Collections.Generic;
using GameSchedulerMicroservice.Models;

namespace GameSchedulerMicroservice.Repositories
{
    public interface IGameScheduleRepository
    {
        void StoreFullSchedule(dynamic response);
        void StoreDailySchedule();
        IList<Message> GetNextGames();
    }
}