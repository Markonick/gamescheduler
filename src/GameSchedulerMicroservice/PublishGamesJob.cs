using System.Threading.Tasks;
using GameSchedulerMicroservice.Repositories;
using Quartz;

namespace GameSchedulerMicroservice
{
    public class PublishGamesJob : IJob
    {
        private readonly IMessageBusSetup _messageBusSetup;
        private readonly IGameScheduleRepository _gameRepo;

        public PublishGamesJob(IGameScheduleRepository gameRepo, IMessageBusSetup messageBusSetup)
        {
            _gameRepo = gameRepo;
            _messageBusSetup = messageBusSetup;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var message = _gameRepo.GetNextGames();
            _messageBusSetup.Publish(message);
            await Task.Delay(1);
        }
    }
}
