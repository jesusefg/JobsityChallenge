using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebApplication.Data.Entities;
using WebApplication.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using RabbitMQ.Client;
using System.Text;

namespace WebApplication.SignalRooms
{
    //Class inherited from SignalR Hub
    public class SignalRoom : Hub
    {
        private readonly ISQLRepository<ChatHistory> _chatRepository;
        private readonly ISQLRepository<IdentityUser> _userRepository;

        public SignalRoom(ISQLRepository<ChatHistory> chatRepository, ISQLRepository<IdentityUser> userRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        public async Task SendMessage(string connectionID, string userId, string message)
        {

            if(message.Length > 0 && message.ToCharArray()[0] == '/')
            {
                SendMessageToKiwi(message);
            }
            else
            {
                string userName = _userRepository.GetAll().Where(x => x.Id == userId).Select(x => x.UserName).FirstOrDefault();

                ChatHistory newChat = new ChatHistory()
                {
                    Message = message,
                    SenderId = userId,
                    TimeStamp = DateTime.Now
                };

                _chatRepository.Insert(newChat);

                await Clients.AllExcept(connectionID).SendAsync("ReceivedMessage", userName, message);
            }

            
        }

        //Kiwi is the name of the bot that will receive the message
        public void SendMessageToKiwi(string message)
        {
            string _url = "amqps://pnceccsp:BFnfl8mUyG67F5oILrS6Z9PX5rfqdcDN@woodpecker.rmq.cloudamqp.com/pnceccsp";
            string queueName = "kiwi";

            var factory = new ConnectionFactory() { Uri = new Uri(_url) };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}
