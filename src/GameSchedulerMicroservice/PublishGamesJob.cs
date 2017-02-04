using System.Threading.Tasks;
using GameScheduler.Repositories;
using GameSchedulerMicroservice;
using Quartz;

namespace GameScheduler
{
    public class PublishGamesJob : IJob
    {
        private readonly Message _message;
        private readonly IMessageBusSetup _messageBusSetup;

        public PublishGamesJob(Message message, IMessageBusSetup messageBusSetup)
        {
            _message = message;
            _messageBusSetup = messageBusSetup;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _messageBusSetup.Publish(_message);
            await Task.Delay(1);
        }
    }
}
