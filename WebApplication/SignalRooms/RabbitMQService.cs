using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using WebApplication.Data.Entities;
using WebApplication.Data.Interfaces;

namespace WebApplication.SignalRooms
{
    public class RabbitMQService : IRabbitMQService
    {
        protected readonly ConnectionFactory _factory;
        protected readonly IConnection _connection;
        protected readonly IModel _readChannel;
        protected readonly IModel _writeChannel;

        protected readonly IServiceProvider _serviceProvider;

        private const string _rabbitMQUrl = "amqps://pnceccsp:BFnfl8mUyG67F5oILrS6Z9PX5rfqdcDN@woodpecker.rmq.cloudamqp.com/pnceccsp";
        private const string _writeQueueName = "kiwi";
        private const string _readQueueName = "talksity";

        public RabbitMQService(IServiceProvider serviceProvider)
        {
            // Opens the connections to RabbitMQ
            _factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQUrl) };
            _connection = _factory.CreateConnection();
            _readChannel = _connection.CreateModel();
            _writeChannel = _connection.CreateModel();

            _serviceProvider = serviceProvider;
        }

        public virtual void Connect()
        {
            // Declare a RabbitMQ Queue
            _readChannel.QueueDeclare(queue: _readQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _writeChannel.QueueDeclare(queue: _writeQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            ReadMessageFromKiwi();
        }

        public void ReadMessageFromKiwi()
        {
            var consumer = new EventingBasicConsumer(_readChannel);
            consumer.Received += delegate (object model, BasicDeliverEventArgs ea)
            {
                var body = ea.Body.ToArray();
                var data = Encoding.UTF8.GetString(body);

                string roomName = data.Split('|')[0];
                string message = data.Split('|')[1];

                string userName = "Kiwi";

                // Get the ChatHub from SignalR (using DI)
                var chatHub = (IHubContext<SignalRoom>)_serviceProvider.GetService(typeof(IHubContext<SignalRoom>));

                // Send message to all users
                chatHub.Clients.Group(roomName).SendAsync("ReceivedMessage", userName, message);

                using(var scope = _serviceProvider.CreateScope())
                {
                    var _chatRepository = scope.ServiceProvider.GetRequiredService<ISQLRepository<ChatHistory>>();
                    var _userRepository = scope.ServiceProvider.GetRequiredService<ISQLRepository<IdentityUser>>();
                    var _roomRepository = scope.ServiceProvider.GetRequiredService<ISQLRepository<ChatRoom>>();

                    int? roomId = _roomRepository.GetAll().Where(x => x.Name == roomName).Select(x => x.Id).FirstOrDefault();

                    if (!roomId.HasValue)
                        return;

                    string userId = _userRepository.GetAll().Where(x => x.UserName == userName).Select(x => x.Id).FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(userId))
                        return;

                    ChatHistory newChat = new ChatHistory()
                    {
                        Message = message,
                        SenderId = userId,
                        TimeStamp = DateTime.Now,
                        RoomId = roomId.Value
                    };

                    _chatRepository.Insert(newChat);
                }
                
            };
            _readChannel.BasicConsume(queue: _readQueueName,
                                    autoAck: true,
                                    consumer: consumer);
        }

        //Kiwi is the name of the bot that will receive the message
        public void SendMessageToKiwi(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _writeChannel.BasicPublish(exchange: "",
                                    routingKey: _writeQueueName,
                                    basicProperties: null,
                                    body: body);
        }
    }
}
