using System.Threading.Tasks;

namespace GameScheduler
{
    public interface IQuartzScheduler
    {
        Task Start();
        Task Stop();
    }
}