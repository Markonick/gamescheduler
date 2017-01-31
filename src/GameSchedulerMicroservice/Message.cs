namespace GameSchedulerMicroservice
{
    public class Message
    {
        public string _Id { get; set; }
        public string Id { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public Team AwayTeam { get; set; }
        public Team HomeTeam { get; set; }
        public string Location { get; set; }
    }
}