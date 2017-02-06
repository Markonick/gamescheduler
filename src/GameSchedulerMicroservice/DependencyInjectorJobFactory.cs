using System;
using Quartz;
using Quartz.Spi;

namespace GameSchedulerMicroservice
{
    public class DependencyInjectorJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyInjectorJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;
            var jobType = jobDetail.JobType;

            return _serviceProvider.GetService(jobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}