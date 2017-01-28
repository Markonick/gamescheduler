using RabbitMQ.Client;

namespace GameSchedulerMicroservice
{
    public interface IMessageBusSetup
    {
        IModel Setup();
    }
}