namespace GameSchedulerMicroservice.Helpers
{
    public class TimeProvider : ITimeProvider
    {
        public string Date { get; set; }

        public TimeProvider(string date)
        {
            Date = date;
        }
    }
}
