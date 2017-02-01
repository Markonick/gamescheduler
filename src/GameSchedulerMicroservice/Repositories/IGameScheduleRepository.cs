using MongoDB.Driver;

namespace GameScheduler.Repositories
{
    public interface IGameScheduleRepository
    {
        void StoreFullSchedule(dynamic response, string collectionName);
        void StoreDailySchedule(string sourceName, string targetName);
    }
}