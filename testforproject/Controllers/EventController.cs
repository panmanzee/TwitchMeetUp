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
            ViewBag.UserId = userId;
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

        [HttpGet("Event/EventScore/{id}")]
        public async Task<IActionResult> EventScore(int id)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var ev = await _db.Events.Include(e => e.Participants)
                                     .FirstOrDefaultAsync(e => e.Eid == id);
            
            if (ev == null) return NotFound();

            // Check if user is a participant
            if (!ev.Participants.Any(u => u.Uid == userId))
            {
                return Forbid();
            }

            // Check if user already scored
            var existingScore = await _db.EventScores.FirstOrDefaultAsync(s => s.EventId == id && s.UserId == userId);
            if (existingScore != null)
            {
                ViewBag.Message = "You have already rated this event.";
            }

            ViewBag.EventId = ev.Eid;
            ViewBag.EventName = ev.Name;

            return View(ev);
        }

        [HttpPost("Event/SubmitEventScore")]
        public async Task<IActionResult> SubmitEventScore(int eventId, int score, string comment)
        {
            var userId = _jwtService.UserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var ev = await _db.Events.Include(e => e.Owner).FirstOrDefaultAsync(e => e.Eid == eventId);
            if (ev == null) return NotFound();

            var existingScore = await _db.EventScores.FirstOrDefaultAsync(s => s.EventId == eventId && s.UserId == userId);
            if (existingScore != null)
            {
                return BadRequest("You have already rated this event.");
            }

            var eventScore = new testforproject.Models.EventScore
            {
                EventId = eventId,
                UserId = (int)userId,
                Score = score,
                Comment = comment
            };

            _db.EventScores.Add(eventScore);
            await _db.SaveChangesAsync();

            // Recalculate HostScore
            var allHostScores = await _db.EventScores
                .Where(s => s.Event.OwnerId == ev.OwnerId)
                .AverageAsync(s => (double?)s.Score) ?? 0;

            var host = await _db.Users.FindAsync(ev.OwnerId);
            if (host != null)
            {
                host.HostScore = (int)Math.Round(allHostScores);
                _db.Users.Update(host);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("EventDetails", new { id = eventId });
        }
    }
}
