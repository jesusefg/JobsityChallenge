using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebApplication.Data.Entities;
using WebApplication.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Threading;

namespace WebApplication.SignalRooms
{
    //Class inherited from SignalR Hub
    public class SignalRoom : Hub
    {
        private readonly ISQLRepository<ChatHistory> _chatRepository;
        private readonly ISQLRepository<IdentityUser> _userRepository;
        private readonly IRabbitMQService _rabbitMQService;

        public SignalRoom(
            ISQLRepository<ChatHistory> chatRepository,
            ISQLRepository<IdentityUser> userRepository,
            IRabbitMQService rabbitMQService)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _rabbitMQService = rabbitMQService;
        }

        public async Task SendMessage(string connectionID, string userId, string message)
        {

            if(message.Length > 0 && message.ToCharArray()[0] == '/')
            {
                _rabbitMQService.SendMessageToKiwi(message);
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
    }
}
