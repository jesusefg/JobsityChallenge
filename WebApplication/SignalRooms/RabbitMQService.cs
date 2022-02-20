using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using WebApplication.Data.Interfaces;

namespace WebApplication.SignalRooms
{
    public class RabbitMQService : IRabbitMQService
    {
        protected readonly ConnectionFactory _factory;
        protected readonly IConnection _connection;
        protected readonly IModel _channel;

        protected readonly IServiceProvider _serviceProvider;

        private const int _sleepTime = 1000 * 3; // 3 seconds
        private const string _rabbitMQUrl = "amqps://pnceccsp:BFnfl8mUyG67F5oILrS6Z9PX5rfqdcDN@woodpecker.rmq.cloudamqp.com/pnceccsp";
        private const string _writeQueueName = "kiwi";
        private const string _readQueueName = "talksity";

        public RabbitMQService(IServiceProvider serviceProvider)
        {
            // Opens the connections to RabbitMQ
            _factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQUrl) };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _serviceProvider = serviceProvider;
        }

        public virtual void Connect()
        {
            // Declare a RabbitMQ Queue
            _channel.QueueDeclare(queue: _readQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            
            do
            {
                ReadMessageFromKiwi();

                Thread.Sleep(_sleepTime);
            } while (true);
        }

        public void ReadMessageFromKiwi()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += delegate (object model, BasicDeliverEventArgs ea)
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Get the ChatHub from SignalR (using DI)
                var chatHub = (IHubContext<SignalRoom>)_serviceProvider.GetService(typeof(IHubContext<SignalRoom>));

                // Send message to all users
                chatHub.Clients.All.SendAsync("ReceivedMessage", "Kiwi", message);
                // send back message to users
            };
            _channel.BasicConsume(queue: _readQueueName,
                                    autoAck: true,
                                    consumer: consumer);
        }

        //Kiwi is the name of the bot that will receive the message
        public void SendMessageToKiwi(string message)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQUrl) };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _writeQueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: _writeQueueName,
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}
