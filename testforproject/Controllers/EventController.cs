using Microsoft.AspNetCore.Mvc;
using testforproject.Authen.Services;
using testforproject.Data;

namespace testforproject.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        public EventController(ApplicationDbContext db, IJwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
        }
        public IActionResult Index()
        {
            var userId = _jwtService.UserId;
            if (userId == null)
            {
                return Redirect("Home/dashboard");
            }
            return View();
        }
        public IActionResult createEvent()
        {
            var userId = _jwtService.UserId;
            if (userId == null)
            {
                return Redirect("Home/dashboard");
            }
            return View();
        }
    }
}
