using RestSharp;

namespace GameSchedulerMicroservice
{
    public interface IGameScheduleWebApiConsumer
    {
        dynamic Get();
    }
}