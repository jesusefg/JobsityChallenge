using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    public class RoomController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
