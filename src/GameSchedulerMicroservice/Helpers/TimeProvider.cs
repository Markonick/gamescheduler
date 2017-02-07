namespace GameSchedulerMicroservice.Helpers
{
    public class TimeProvider : ITimeProvider
    {
        public string Time { get; set; }

        public TimeProvider(string time)
        {
            Time = time;
        }
    }
}
