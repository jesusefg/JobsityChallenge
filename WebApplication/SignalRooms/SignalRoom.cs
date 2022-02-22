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
        private readonly ISQLRepository<ChatRoom> _roomRepository;
        private readonly IRabbitMQService _rabbitMQService;

        public SignalRoom(
            ISQLRepository<ChatHistory> chatRepository,
            ISQLRepository<IdentityUser> userRepository,
            ISQLRepository<ChatRoom> roomRepository,
            IRabbitMQService rabbitMQService)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _roomRepository = roomRepository;
            _rabbitMQService = rabbitMQService;
        }

        public async Task SendMessage(string connectionId, string roomName, string userId, string message)
        {
            int? roomId = _roomRepository.GetAll().Where(x => x.Name == roomName).Select(x => x.Id).FirstOrDefault();

            if (!roomId.HasValue)
                return;

            if(message.Length > 0 && message.ToCharArray()[0] == '/')
            {
                _rabbitMQService.SendMessageToKiwi(string.Format("{0}|{1}", roomName, message));
            }
            else
            {
                string userName = _userRepository.GetAll().Where(x => x.Id == userId).Select(x => x.UserName).FirstOrDefault();

                ChatHistory newChat = new ChatHistory()
                {
                    Message = message,
                    SenderId = userId,
                    TimeStamp = DateTime.Now,
                    RoomId = roomId.Value
                };

                _chatRepository.Insert(newChat);

                await Clients.GroupExcept(roomName, connectionId).SendAsync("ReceivedMessage", userName, message);
            }

        }

        public async Task Join(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }
    }
}
