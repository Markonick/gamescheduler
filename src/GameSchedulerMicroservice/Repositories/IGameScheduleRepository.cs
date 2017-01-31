using MongoDB.Driver;

namespace GameScheduler.Repositories
{
    public interface IGameScheduleRepository
    {
        IMongoDatabase  Setup();
    }
}