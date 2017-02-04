namespace GameScheduler.Repositories
{
    public interface IGameScheduleRepository
    {
        void StoreFullSchedule(dynamic response);
        void StoreDailySchedule();
        Message GetNextGames();
    }
}