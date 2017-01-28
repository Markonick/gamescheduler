using System;
using RestSharp;

namespace GameSchedulerMicroservice
{
    public class GameScheduleWebApiConsumer : IGameScheduleWebApiConsumer
    {
        private readonly IRestClient _client;
        private readonly string _url;
        private readonly string _format;
        private readonly string _seasonName;

        public GameScheduleWebApiConsumer(IRestClient client, string url, string format, string seasonName)
        {
            _client = client;
            _url = url;
            _format = format;
            _seasonName = seasonName;
        }

        public IRestResponse Get()
        {
            var request = new RestRequest(_url, Method.GET) { RequestFormat = DataFormat.Json };

            request.AddUrlSegment("season-name", _seasonName);
            request.AddUrlSegment("format", _format);

            var response = _client.Execute<dynamic>(request);

            if (response.Data == null)
            {
                throw new Exception(response.ErrorMessage);
            }

            return response;
        }
    }
}