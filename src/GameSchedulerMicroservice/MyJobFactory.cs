using System;
using Quartz;
using Quartz.Spi;

namespace GameSchedulerMicroservice
{
    public class MyJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MyJobFactory(IServiceProvider serviceProvider)
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