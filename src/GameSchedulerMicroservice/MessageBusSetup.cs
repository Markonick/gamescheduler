using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace GameSchedulerMicroservice
{
    public class MessageBusSetup : IMessageBusSetup
    {
        private readonly string _exchange;
        private readonly string _exchangeType;
        private readonly string _queueName;
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private readonly int _reconnect;
        private string _connectionName;

        public MessageBusSetup(string host, string username, string password, int reconnect, string exchange, string connectionName, string queueName, string exchangeType)
        {
            _host = host;
            _username = username;
            _password = password;
            _reconnect = reconnect;
            _exchange = exchange;
            _connectionName = connectionName;
            _queueName = queueName;
            _exchangeType = exchangeType;

            Setup();
        }

        public void Publish<T>(T message)
        {
            var connectionFactory = ConnectionBuilder();
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(_exchange, string.Empty, properties, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            }
        }
           
        private void Setup()
        {
            var connectionFactory = ConnectionBuilder();

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.ExchangeDeclare(_exchange, _exchangeType,  true);
                channel.QueueDeclare(_queueName,  true,  false,  false);
                channel.QueueBind(_queueName, _exchange,  string.Empty);
            }
        }

        private ConnectionFactory ConnectionBuilder()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = _host,
                UserName = _username,
                Password = _password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_reconnect)
            };
            return connectionFactory;
        }
    }
}
