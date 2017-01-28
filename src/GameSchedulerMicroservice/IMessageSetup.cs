using RabbitMQ.Client;

namespace GameSchedulerMicroservice
{
    public interface IMessageSetup
    {
        IModel Setup();
    }
}