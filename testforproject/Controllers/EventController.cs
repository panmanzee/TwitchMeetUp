using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testforproject.Authen.Services;
using testforproject.Data;

namespace testforproject.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwtService;
        private readonly ILogger<EventController> _logger;
        public EventController(ApplicationDbContext db, IJwtService jwtService, ILogger<EventController> logger)
        {
            _db = db;
            _jwtService = jwtService;
            _logger = logger;
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
        public async Task<IActionResult> EventDetails(int id)
        {
            var eventDetail = await _db.Events
                .Include(e => e.Owner)
                .Include(e => e.Participants)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.Eid == id);

            if (eventDetail == null)
                return NotFound();


            var userId = _jwtService.UserId;
            ViewBag.IsLoggedIn = userId != null;
            ViewBag.IsJoined = userId != null &&
                               eventDetail.Participants.Any(u => u.Uid == userId);

            return View(eventDetail);
        }

        [Route("Event/ManageEvent/{id}")]
        public async Task<IActionResult> ManageEvent(int id)
        {

            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (ev.OwnerId != _jwtService.UserId)
            {
                return NotFound();
            }
            return View(ev);
        }


    }
}
