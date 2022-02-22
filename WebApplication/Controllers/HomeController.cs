using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebApplication.Data.Entities;
using WebApplication.Data.Interfaces;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISQLRepository<ChatRoom> _roomRepository;

        public HomeController(ISQLRepository<ChatRoom> roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public IActionResult Index()
        {
            List<ChatRoomListModel> model = _roomRepository.GetAll()
                                        .Select(x => new ChatRoomListModel
                                        {
                                            Name = x.Name
                                        }).ToList();

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
