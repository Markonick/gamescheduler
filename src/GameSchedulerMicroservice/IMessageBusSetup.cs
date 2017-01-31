namespace GameSchedulerMicroservice
{
    public interface IMessageBusSetup
    {
        void Publish<T>(T message);
    }
}