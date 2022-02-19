using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebApplication.Data.Entities;
using WebApplication.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq;

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
            ChatHistory newChat = new ChatHistory()
            {
                Message = message,
                SenderId = userId,
                TimeStamp = DateTime.Now
            };

            _chatRepository.Insert(newChat);

            string userName = _userRepository.GetAll().Where(x => x.Id == userId).Select(x => x.UserName).FirstOrDefault();

            await Clients.AllExcept(connectionID).SendAsync("ReceivedMessage", userName, message);
        }
    }
}
