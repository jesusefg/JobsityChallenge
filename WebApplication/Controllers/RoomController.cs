using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApplication.Data.Entities;
using WebApplication.Data.Interfaces;
using System.Linq;
using WebApplication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApplication.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private readonly ISQLRepository<ChatHistory> _chatRepository;
        private readonly ISQLRepository<ChatRoom> _roomRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public RoomController(
            ISQLRepository<ChatHistory> chatRepository,
            ISQLRepository<ChatRoom> roomRepository,
            UserManager<IdentityUser> userManager)
        {
            _chatRepository = chatRepository;
            _roomRepository = roomRepository;
            _userManager = userManager;
        }

        public IActionResult Index(string roomName)
        {
            List<ChatHistoryModel> model = _chatRepository.GetAll().Include(x => x.Sender).Include(x => x.Room)
                                        .Where(x => x.Room.Name == roomName)
                                        .OrderByDescending(x => x.TimeStamp)
                                        .Take(50)
                                        .Select(x => new ChatHistoryModel 
                                        {
                                            Message = x.Message,
                                            SenderId = x.SenderId,
                                            TimeStamp = x.TimeStamp,
                                            SenderName = x.Sender.UserName
                                        }).ToList().OrderBy(x => x.TimeStamp).ToList();

            ViewBag.roomName = roomName;

            return View(model);
        }

        [HttpPost]
        public string CreateRoom(string newRoomName)
        {
            if(_roomRepository.GetAll().Any(x => x.Name == newRoomName))
            {
                return "A room with that name is already created";
            }

            ChatRoom newRoom = new ChatRoom()
            {
                Name = newRoomName,
                CreatedById = _userManager.GetUserAsync(User).Result.Id
            };

            _roomRepository.Insert(newRoom);

            return "success";
        }
    }
}
