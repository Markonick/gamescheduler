using RestSharp;

namespace GameSchedulerMicroservice
{
    public interface IGameScheduleWebApiConsumer
    {
        IRestResponse Get();
    }
}