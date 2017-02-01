using MongoDB.Driver;

namespace GameScheduler.Repositories
{
    public interface IGameScheduleRepository
    {
        IMongoDatabase Db { get; set; }
        void StoreFullSchedule();
        void StoreDailySchedule();
    }
}