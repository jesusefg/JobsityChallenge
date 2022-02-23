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
using WebApplication.Helpers;

namespace WebApplication.SignalRooms
{
    public class RabbitMQService : IRabbitMQService
    {
        protected readonly ConnectionFactory _factory;
        protected readonly IConnection _connection;
        protected readonly IModel _readChannel;
        protected readonly IModel _writeChannel;

        protected readonly IServiceProvider _serviceProvider;

        private readonly string _rabbitMQUrl;
        private const string _writeQueueName = "kiwi";
        private const string _readQueueName = "talksity";
        private const string botUserName = "Kiwi";

        public RabbitMQService(IServiceProvider serviceProvider)
        {
            _rabbitMQUrl = CryptoHelper.DecryptString("FVzeB0dh8GlZ8OlPv0nKgQYEq6pPMoUKDkz3b3u0CeMjLyGN39/kV/BuI5cXal6T7x+/m7hMnRgAdeN3F9nv+izRTcEv/FgMgR0a62wj2s7iAnCt5h07xunHIpGQKvx+1i/E8GbgBNOCUlL2xFO5fw==|TcdpGHB1jCUHb2WLVYJ17JD/N3g2pJd7hUO79St0UwU=");

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

                

                // Get the ChatHub from SignalR (using DI)
                var chatHub = (IHubContext<SignalRoom>)_serviceProvider.GetService(typeof(IHubContext<SignalRoom>));

                // Send message to all users
                chatHub.Clients.Group(roomName).SendAsync("ReceivedMessage", botUserName, message);


                //We must create a scope to access the database
                using(var scope = _serviceProvider.CreateScope())
                {
                    var _chatRepository = scope.ServiceProvider.GetRequiredService<ISQLRepository<ChatHistory>>();
                    var _userRepository = scope.ServiceProvider.GetRequiredService<ISQLRepository<IdentityUser>>();
                    var _roomRepository = scope.ServiceProvider.GetRequiredService<ISQLRepository<ChatRoom>>();

                    int? roomId = _roomRepository.GetAll().Where(x => x.Name == roomName).Select(x => x.Id).FirstOrDefault();

                    if (!roomId.HasValue)
                        return;

                    string userId = _userRepository.GetAll().Where(x => x.UserName == botUserName).Select(x => x.Id).FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(userId))
                        return;

                    // save the bot post into the database
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
