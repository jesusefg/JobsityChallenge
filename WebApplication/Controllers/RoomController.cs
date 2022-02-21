using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApplication.Data.Entities;
using WebApplication.Data.Interfaces;
using System.Linq;
using WebApplication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private readonly ISQLRepository<ChatHistory> _chatRepository;

        public RoomController(ISQLRepository<ChatHistory> chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public IActionResult Index()
        {
            List<ChatHistoryModel> Model = _chatRepository.GetAll().Include(x => x.Sender)
                                        .OrderByDescending(x => x.TimeStamp)
                                        .Take(50)
                                        .Select(x => new ChatHistoryModel 
                                        {
                                            Message = x.Message,
                                            SenderId = x.SenderId,
                                            TimeStamp = x.TimeStamp,
                                            SenderName = x.Sender.UserName
                                        }).ToList().OrderBy(x => x.TimeStamp).ToList();
            return View(Model);
        }
    }
}
